using System.Collections.Generic;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    static public TorchManager instance;

    [Header("External References")]
    [SerializeField] private TorchConfig _torchConfig;

    // -- Private Variables --
    List<Torch> ActiveTorchs = new List<Torch>();

    private void Awake()
    {
        instance = this;
    }

    public void AddNewTorchToList(Torch _newTorch)
    {
        ActiveTorchs.Insert(0,_newTorch);

        while (ActiveTorchs.Count > _torchConfig.maxNumberTorch)
        {
            ActiveTorchs[4].DeactivateTorch();
            RemoveTorchFromList(ActiveTorchs[4]);
        }
    }

    public void RemoveTorchFromList(Torch _removeTorch)
    {
        ActiveTorchs.Remove(_removeTorch);
    }
}
