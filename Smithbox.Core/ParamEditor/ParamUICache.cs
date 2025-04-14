using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.ParamEditorNS;

public class ParamUICache
{
    public Project Project { get; set; }

    public ParamUICache(Project curProject)
    {
        Project = curProject;
    }

    private Dictionary<(object, string), object> caches = new();

    /// <summary>
    /// Gets/Sets a cache. The cached data is intended to have a lifetime until the contextual object is modified, or the UIScreen object is refreshed.
    /// </summary>
    public T GetCached<T>(object context, Func<T> getValue)
    {
        return GetCached(context, "", getValue);
    }

    /// <summary>
    /// Gets/Sets a cache with a specific key, avoiding any case where there would be conflict over the context-giving object
    /// </summary>
    public T GetCached<T>(object context, string key, Func<T> getValue)
    {
        (object context, string key) trueKey = (context, key);
        if (!caches.ContainsKey(trueKey))
        {
            caches[trueKey] = getValue();
        }

        return (T)caches[trueKey];
    }

    /// <summary>
    /// Removes cached data related to the context object
    /// </summary>
    public void RemoveCache(object context)
    {
        IEnumerable<KeyValuePair<(object, string), object>> toRemove =
            caches.Where(keypair => keypair.Key.Item2 == context);

        foreach (KeyValuePair<(object, string), object> kp in toRemove)
        {
            caches.Remove(kp.Key);
        }
    }

    /// <summary>
    /// Removes cached data within the UIScreen's domain
    /// </summary>
    public void RemoveCache()
    {
        foreach (KeyValuePair<(object, string), object> kp in caches)
        {
            caches.Remove(kp.Key);
        }
    }

    /// <summary>
    /// Clears all caches
    /// </summary>
    public void ClearCaches()
    {
        caches.Clear();
    }
}
