namespace Template.Core.CrossCutting.Exceptions
{

    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entity) : base($"entity {entity} not found"){}
        public EntityNotFoundException(Type entity) : base($"entity {entity.Name} not found"){}
        public EntityNotFoundException(string entity, string id) : base($"entity {entity} with id: {id} not found"){}
    }
    public class EntityDeactivatedException : Exception
    {
        public EntityDeactivatedException(string entity, string id) : base($"entity {entity} with id: {id} is deactivated"){}
    }
    public class BusinessRuleException(string rule) : Exception($"{rule}"){}
}
