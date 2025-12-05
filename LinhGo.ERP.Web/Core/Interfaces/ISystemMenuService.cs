using LinhGo.ERP.Web.Core.Models;

namespace LinhGo.ERP.Web.Core.Interfaces;

public interface ISystemMenuService
{
    Task<IEnumerable<PanelMenu>> GetAllPanelMenus();
    Task<IEnumerable<PanelMenu>> FilterPanelMenus(string term);
    Task<PanelMenu?> GetCurrentPanelMenu(Uri uri);
}