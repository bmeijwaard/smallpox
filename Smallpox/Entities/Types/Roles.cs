using System.ComponentModel.DataAnnotations;

namespace Smallpox.Entities.Types
{
    public enum Roles
    {
        [Display(Name = "Patient")]
        Patient = 0,

        [Display(Name = "Intern")]
        Intern = 10,

        [Display(Name = "Analist")]
        Analist = 20,

        [Display(Name = "Reseacher")]
        Reseacher = 30,

        [Display(Name = "Surgeon")]
        Surgeon = 50,

        [Display(Name = "Administrator")]
        Administrator = 1000
    }
}
