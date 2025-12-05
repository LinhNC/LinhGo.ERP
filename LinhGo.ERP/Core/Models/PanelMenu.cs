namespace LinhGo.ERP.Core.Models;

public class PanelMenu
{
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public string? Path { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool New { get; set; }
    public bool Updated { get; set; }
    public bool Pro { get; set; }
    public bool Expanded { get; set; }
    public IEnumerable<PanelMenu>? Children { get; set; }
    public IEnumerable<string>? Tags { get; set; }
    public IEnumerable<MenuSection>? Toc { get; set; }
}

public class MenuSection
{
    public required string Text { get; set; }
    public required string Anchor { get; set; }
}
