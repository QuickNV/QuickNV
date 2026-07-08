
namespace QuickNV.Model;

public class ModelChecker<T>
        where T : class
{
    private bool notChangeWhenNewValueNull = false;

    public class PropertyInfo
    {
        public Func<T, object> Getter { get; set; }
        public Action<T, object> Setter { get; set; }

        public PropertyInfo(Func<T, object> getter, Action<T, object> setter)
        {
            Getter = getter;
            Setter = setter;
        }
    }

    private PropertyInfo[] properties;

    public ModelChecker(bool notChangeWhenNewValueNull, params PropertyInfo[] properties)
    {
        this.notChangeWhenNewValueNull = notChangeWhenNewValueNull;
        this.properties = properties;
    }

    /// <summary>
    /// 检查与修改
    /// </summary>
    /// <param name="newModel"></param>
    /// <param name="existModel"></param>
    /// <returns>是否有修改</returns>
    public bool CheckAndModifyModel(T newModel, T existModel)
    {
        bool isChanged = false;
        foreach (var prop in properties)
        {
            var newValue = prop.Getter(newModel);
            var existValue = prop.Getter(existModel);
            //没有变化
            if (newValue == null && (notChangeWhenNewValueNull||existValue == null))
                continue;

            //如果值都不为NULL
            if (newValue != null && existValue != null)
            {
                //值相同
                if (newValue.Equals(existValue))
                    continue;
            }
            prop.Setter(existModel, newValue);
            isChanged = true;
        }
        return isChanged;
    }

    public void CheckModels(T[] existModels, T[] newModels, out List<T> addList, out List<T> updateList, out List<T> deleteList)
    {
        addList = new();
        updateList = new();
        deleteList = new();

        var existModelDict = existModels.ToDictionary(t => t, t => t);
        var newModelDict = newModels.ToDictionary(t => t, t => t);

        foreach (var existModel in existModelDict.Keys)
        {
            if (newModelDict.TryGetValue(existModel, out var newModel))
            {
                if (CheckAndModifyModel(newModel, existModel))
                    updateList.Add(existModel);
            }
            else
            {
                deleteList.Add(existModel);
            }
        }
        foreach (var newModel in newModelDict.Keys)
        {
            if (existModelDict.ContainsKey(newModel))
                continue;
            addList.Add(newModel);
        }
    }
}