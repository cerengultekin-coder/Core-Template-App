namespace Core.AI.FunctionCalling.FunctionSchema;

public static class FunctionSchemaGenerator
{
    public static object ToOpenAISchema(IAiFunction f)
    {
        var parameters = f is IJsonSchemaProvider sp ? sp.GetJsonSchema() : new { type = "object", properties = new { }, required = new string[] { } };

        return new
        {
            name = f.Name,
            description = f.Description,
            parameters
        };
    }

    public static object[] ToOpenAISchemas(IEnumerable<IAiFunction> functions)
    => functions.Select(ToOpenAISchema).ToArray();

    public static List<object> ToOpenRouterTools(IEnumerable<IAiFunction> functions)
    {
        return functions.Select(f => new
        {
            type = "function",
            function = new
            {
                name = f.Name,
                description = f.Description,
                parameters = (f is IJsonSchemaProvider sp && sp.GetJsonSchema() != null)
        ? sp.GetJsonSchema()
        : new { type = "object", properties = new { }, required = Array.Empty<string>() }
            }
        }).Cast<object>().ToList();
    }

    public static List<object> ToOllamaSchemas(IEnumerable<IAiFunction> functions)
    {
        return functions.Select(f => new
        {
            name = f.Name,
            description = f.Description,
            parameters = (f is IJsonSchemaProvider sp && sp.GetJsonSchema() != null)
        ? sp.GetJsonSchema()
        : new { type = "object", properties = new { }, required = Array.Empty<string>() }
        }).Cast<object>().ToList();
    }
}