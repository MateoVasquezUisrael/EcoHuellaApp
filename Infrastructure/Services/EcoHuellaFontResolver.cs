using PdfSharpCore.Fonts;

namespace EcoHuellaApp.Infrastructure.Services;

public sealed class EcoHuellaFontResolver : IFontResolver
{
    public const string FamilyName = "OpenSans";

    private const string CaraRegular = "OpenSansRegular";
    private const string CaraSemibold = "OpenSansSemibold";

    private readonly byte[] _regular;
    private readonly byte[] _semibold;

    public string DefaultFontName => CaraRegular;

    public EcoHuellaFontResolver(byte[] regular, byte[] semibold)
    {
        _regular = regular;
        _semibold = semibold;
    }

    public byte[] GetFont(string faceName) => faceName switch
    {
        CaraSemibold => _semibold,
        _ => _regular
    };

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
        new(isBold ? CaraSemibold : CaraRegular);
}
