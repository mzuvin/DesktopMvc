using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Web.Helper;

public class EmbeddedFileProvider : IFileProvider
{
    private readonly Assembly _assembly;

    public EmbeddedFileProvider(Assembly assembly)
    {
        _assembly = assembly;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var resourceName = $"{_assembly.GetName().Name}.wwwroot{subpath.Replace('/', '.')}";
        var resourceStream = _assembly.GetManifestResourceStream(resourceName);
        
        if (resourceStream == null)
        {
            return new NotFoundFileInfo(subpath);
        }

        return new EmbeddedFileInfo(resourceStream,resourceName);
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return new NotFoundDirectoryContents();
    }

    public IChangeToken Watch(string filter)
    {
        return NullChangeToken.Singleton;
    }
}

public class EmbeddedFileInfo : IFileInfo
{
    private readonly Stream _stream;
    private readonly string _fileName;

    public EmbeddedFileInfo(Stream stream, string resourceName)
    {
        _stream = stream;
        _fileName = Path.GetFileName(resourceName);
    }

    public bool Exists => true;
    public bool IsDirectory => false; 
    public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
    public long Length => _stream.Length;
    public string PhysicalPath => null;
    public string Name => _fileName;
    public Stream CreateReadStream() => _stream;
}