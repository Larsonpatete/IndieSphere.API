namespace IndieSphere.Application;

public sealed record IndieSphereLink(Uri Uri)
{
    public Uri AtEndpoint(string relativeUrl) => new(Uri, relativeUrl);
}