namespace MicroService.Login.WebApi.Models
{
    public class ResultModel<TEntity>
    {
        public bool    Success { get; set; }
        public TEntity Data    { get; set; }
    }
}