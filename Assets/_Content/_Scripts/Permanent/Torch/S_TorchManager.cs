using System.Collections.Generic;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private RSO_TorchManager _rsoTorchManager;
    [SerializeField] private SSO_Torch _torchConfig;

    // -- Private Variables --
    List<Torch> ActiveTorchs = new List<Torch>();

    private void Awake()
    {
        _rsoTorchManager.value = this;
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
