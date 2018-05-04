using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Smallpox.Helpers.Identity
{
    public class PasswordHasherOptions
    {
        private static readonly RandomNumberGenerator DefaultRng = RandomNumberGenerator.Create();

        public PasswordHasherCompatibilityMode CompatibilityMode { get; set; } = PasswordHasherCompatibilityMode.IdentityV3;

        public int IterationCount { get; set; } = 10000;

        internal RandomNumberGenerator Rng { get; set; } = DefaultRng;
    }

    public class PasswordHasher
    {
        private readonly PasswordHasherCompatibilityMode _compatibilityMode;
        private readonly int _iterCount;
        private readonly RandomNumberGenerator _rng;

        public PasswordHasher(IOptions<PasswordHasherOptions> optionsAccessor = null)
        {
            PasswordHasherOptions passwordHasherOptions = optionsAccessor?.Value ?? new PasswordHasherOptions();
            _compatibilityMode = passwordHasherOptions.CompatibilityMode;
            switch (_compatibilityMode)
            {
                case PasswordHasherCompatibilityMode.IdentityV2:
                    _rng = passwordHasherOptions.Rng;
                    break;
                case PasswordHasherCompatibilityMode.IdentityV3:
                    _iterCount = passwordHasherOptions.IterationCount;
                    if (_iterCount < 1)
                        throw new InvalidOperationException();
                    goto case PasswordHasherCompatibilityMode.IdentityV2;
                default:
                    throw new InvalidOperationException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null || a.Length != b.Length)
                return false;
            bool flag = true;
            for (int index = 0; index < a.Length; ++index)
                flag &= a[index] == b[index];
            return flag;
        }


        public virtual string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");
            if (_compatibilityMode == PasswordHasherCompatibilityMode.IdentityV2)
                return Convert.ToBase64String(HashPasswordV2(password, _rng));
            return Convert.ToBase64String(HashPasswordV3(password, _rng));
        }

        private static byte[] HashPasswordV2(string password, RandomNumberGenerator rng)
        {
            byte[] numArray1 = new byte[16];
            rng.GetBytes(numArray1);
            byte[] numArray2 = KeyDerivation.Pbkdf2(password, numArray1, KeyDerivationPrf.HMACSHA1, 1000, 32);
            byte[] numArray3 = new byte[49];
            numArray3[0] = 0;
            Buffer.BlockCopy(numArray1, 0, numArray3, 1, 16);
            int srcOffset = 0;
            byte[] numArray4 = numArray3;
            int dstOffset = 17;
            int count = 32;
            Buffer.BlockCopy(numArray2, srcOffset, numArray4, dstOffset, count);
            return numArray3;
        }

        private byte[] HashPasswordV3(string password, RandomNumberGenerator rng)
        {
            return HashPasswordV3(password, rng, KeyDerivationPrf.HMACSHA256, _iterCount, 16, 32);
        }

        private static byte[] HashPasswordV3(string password, RandomNumberGenerator rng, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
        {
            byte[] numArray1 = new byte[saltSize];
            rng.GetBytes(numArray1);
            byte[] numArray2 = KeyDerivation.Pbkdf2(password, numArray1, prf, iterCount, numBytesRequested);
            byte[] buffer = new byte[13 + numArray1.Length + numArray2.Length];
            buffer[0] = 1;
            WriteNetworkByteOrder(buffer, 1, (uint)prf);
            WriteNetworkByteOrder(buffer, 5, (uint)iterCount);
            WriteNetworkByteOrder(buffer, 9, (uint)saltSize);
            Buffer.BlockCopy(numArray1, 0, buffer, 13, numArray1.Length);
            Buffer.BlockCopy(numArray2, 0, buffer, 13 + saltSize, numArray2.Length);
            return buffer;
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return (uint)(buffer[offset] << 24 | buffer[offset + 1] << 16 | buffer[offset + 2] << 8) | buffer[offset + 3];
        }

        public virtual PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
                throw new ArgumentNullException("hashedPassword");
            if (providedPassword == null)
                throw new ArgumentNullException("providedPassword");
            byte[] hashedPassword1 = Convert.FromBase64String(hashedPassword);
            if (hashedPassword1.Length == 0)
                return PasswordVerificationResult.Failed;
            switch (hashedPassword1[0])
            {
                case 0:
                    if (!VerifyHashedPasswordV2(hashedPassword1, providedPassword))
                        return PasswordVerificationResult.Failed;
                    return _compatibilityMode != PasswordHasherCompatibilityMode.IdentityV3 ? PasswordVerificationResult.Success : PasswordVerificationResult.SuccessRehashNeeded;
                case 1:
                    int iterCount;
                    if (!VerifyHashedPasswordV3(hashedPassword1, providedPassword, out iterCount))
                        return PasswordVerificationResult.Failed;
                    return iterCount >= _iterCount ? PasswordVerificationResult.Success : PasswordVerificationResult.SuccessRehashNeeded;
                default:
                    return PasswordVerificationResult.Failed;
            }
        }

        private static bool VerifyHashedPasswordV2(byte[] hashedPassword, string password)
        {
            if (hashedPassword.Length != 49)
                return false;
            byte[] salt = new byte[16];
            Buffer.BlockCopy(hashedPassword, 1, salt, 0, salt.Length);
            byte[] b = new byte[32];
            Buffer.BlockCopy(hashedPassword, 1 + salt.Length, b, 0, b.Length);
            return ByteArraysEqual(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA1, 1000, 32), b);
        }

        private static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, out int iterCount)
        {
            iterCount = 0;
            try
            {
                KeyDerivationPrf prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
                iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
                int length = (int)ReadNetworkByteOrder(hashedPassword, 9);
                if (length < 16)
                    return false;
                byte[] salt = new byte[length];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);
                int numBytesRequested = hashedPassword.Length - 13 - salt.Length;
                if (numBytesRequested < 16)
                    return false;
                byte[] b = new byte[numBytesRequested];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, b, 0, b.Length);
                return ByteArraysEqual(KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested), b);
            }
            catch
            {
                return false;
            }
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)value;
        }
    }
}
