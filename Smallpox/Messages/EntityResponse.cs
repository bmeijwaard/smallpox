namespace Smallpox.Messages
{
    public class EntityResponse<TEntity> : ServiceResponse
    {
        public EntityResponse()
        {

        }
        public EntityResponse(TEntity entity)
        {
            Entity = entity;
        }

        public EntityResponse(string errorMessage)
            : base(errorMessage)
        {
        }

        public EntityResponse(IServiceResponse response)
        : base(response.ErrorMessage)
        {
        }

        public EntityResponse(TEntity entity, string errorMessage) : base(errorMessage)
        {
            Entity = entity;
        }

        public TEntity Entity { get; set; }
    }
}
