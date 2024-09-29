namespace TidyHPC.Routers.Urls.Interfaces;

/// <summary>
/// 服务接口
/// </summary>
public interface IServer
{
    /// <summary>
    /// 获取下一个会话
    /// </summary>
    /// <returns></returns>
    Task<Session> GetNextSession();
}
