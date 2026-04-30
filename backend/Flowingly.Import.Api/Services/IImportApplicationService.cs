using Flowingly.Import.Api.Contracts;

namespace Flowingly.Import.Api.Services;

public interface IImportApplicationService
{
    ParseResponse Parse(string text, decimal? taxRate = null);
}
