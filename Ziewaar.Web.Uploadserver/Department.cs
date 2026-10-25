using Ziewaar.TtLog;
using Ziewaar.TtLog.LogFormats;

namespace Ziewaar.Web.Uploadserver;

public class Department
{
    public static readonly LogDepartment Upload = new LogDepartment(mutes: LogLevel.Tech);
}