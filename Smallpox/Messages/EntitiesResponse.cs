using System.Collections.Generic;

namespace Smallpox.Messages
{
    public class EntitiesResponse<TEntity> : ServiceResponse
    {
        public EntitiesResponse(IEnumerable<TEntity> entities)
        {
            Entities = entities;
        }

        public EntitiesResponse(string errorMessage)
            : base(errorMessage)
        {
        }

        public EntitiesResponse(IServiceResponse response)
            : base(response.ErrorMessage)
        {
        }

        public IEnumerable<TEntity> Entities { get; set; }
    }


    /// <summary>
    /// Creates a meaningful response when returning from method in service layer
    /// </summary>
    /// <typeparam name="TEntity">The entity fetched from the DB</typeparam>
    /// <typeparam name="TObject">An additional object containing information that is not stored in the entity</typeparam>
    public class EntitiesResponse<TEntity, TObject> : EntitiesResponse<TEntity>
    {
        public EntitiesResponse(IEnumerable<TEntity> entities) : base(entities)
        {
        }

        public EntitiesResponse(string errorMessage) : base(errorMessage)
        {
        }

        public EntitiesResponse(IServiceResponse response) : base(response)
        {
        }

        public EntitiesResponse(IEnumerable<TEntity> entities, TObject obj) : base(entities)
        {
            Object = obj;
        }

        public TObject Object { get; set; }
    }
}
