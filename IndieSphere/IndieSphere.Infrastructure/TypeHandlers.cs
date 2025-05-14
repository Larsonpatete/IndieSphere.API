using Dapper;
using System.Data;

namespace IndieSphere.Infrastructure;

public static class TypeHandlers
{
    public static void Register()
    {
        // Register custom type handlers here
        var registerType = typeof(SqlMapper.TypeHandler<>);

        var handlerTypes = typeof(TypeHandlers).Assembly.ExportedTypes
            .Where(t => t.BaseType is not null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == registerType)
            .Select(type => (type.BaseType!.GetGenericArguments()[0], type.GetConstructor(Type.EmptyTypes)!.Invoke(null) as SqlMapper.ITypeHandler));

        foreach (var (type, handler) in handlerTypes)
        {
            SqlMapper.AddTypeHandler(type, handler);
        }
    }
}

//public class  UserTypeHandler : SqlMapper.TypeHandler<User>
//{
//    public override User Parse(object value)
//    {
//        if (value is null)
//            throw new ArgumentNullException(nameof(value));
//        return new User((string)value);
//    }
//    public override void SetValue(IDbDataParameter parameter, User value)
//    {
//        if (parameter is null)
//            throw new ArgumentNullException(nameof(parameter));
//        if (value is null)
//            throw new ArgumentNullException(nameof(value));
//        parameter.Value = value.ToString();
//    }
//}
