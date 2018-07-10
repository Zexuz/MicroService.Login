namespace MicroService.Login.WebApi.Models
{
    public class ErrorResult<TEntity>
    {
        public bool    Success => false;
        public TEntity Error   { get; set; }
    }

    public class SuccessResult<TEntitey>
    {
        public bool     Success => true;
        public TEntitey Data    { get; set; }
    }
}