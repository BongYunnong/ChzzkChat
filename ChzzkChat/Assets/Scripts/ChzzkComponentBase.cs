using System.Collections.Generic;
using UnityEngine;

public class ChzzkComponentBase : MonoBehaviour
{
    protected ChzzkController cachedChzzkController = null;
    protected Dictionary<string, string> parameters = new Dictionary<string, string>();
    
    public void InitializeComponent(ChzzkController chzzkController)
    {
        cachedChzzkController = chzzkController;
    }

    public virtual void DoAction(string action, string parameter)
    {
        Debug.LogWarning($"[ChzzkComponent] DoAction called. {GetAPICategory().ToString()} - {(action == null ? "null" : action)}");

        parameters.Clear();
        if (parameter == null)
        {
            return;
        }
        string[] actionParams = parameter.Split(';');
        for (int i = 0; i < actionParams.Length; i++)
        {
            string[] tokens = actionParams[i].Split('=');
            parameters.Add(tokens[0], tokens[1]);
        }
    }

    public virtual EAPICategory GetAPICategory()
    {
        return EAPICategory.Default;
    }

    public string GetParamValue(string key)
    {
        return parameters[key];
    }
}
