using LinhGo.ERP.Core.Models;

namespace LinhGo.ERP.Core.Interfaces;

public interface ISystemMenuService
{
    Task<IEnumerable<PanelMenu>> GetAllPanelMenus();
    Task<IEnumerable<PanelMenu>> FilterPanelMenus(string term);
    Task<PanelMenu?> GetCurrentPanelMenu(Uri uri);
}