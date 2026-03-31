using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBarraOrdenTurno : MonoBehaviour
{
    public GameObject prefabTarjetaBarraOrdenTurno;
    public List<Unidad> lUnidades = new List<Unidad>();
    private readonly List<GameObject> tarjetasInstanciadas = new List<GameObject>();
    private readonly List<UITarjetaBarraOrdenTurno> tarjetasComponentes = new List<UITarjetaBarraOrdenTurno>();

    public void ActualizarBarraOrdenTurno()
    {
        BattleManager battleManager = BattleManager.Instance;
        if (battleManager == null)
        {
            return;
        }

        lUnidades.Clear();
        lUnidades.AddRange(battleManager.lUnidadesTotal);

        AsegurarCapacidadTarjetas(lUnidades.Count);

        for (int i = 0; i < tarjetasInstanciadas.Count; i++)
        {
            GameObject tarjetaGo = tarjetasInstanciadas[i];
            if (tarjetaGo == null)
            {
                continue;
            }

            bool debeMostrarse = i < lUnidades.Count;
            if (tarjetaGo.activeSelf != debeMostrarse)
            {
                tarjetaGo.SetActive(debeMostrarse);
            }

            if (!debeMostrarse)
            {
                continue;
            }

            tarjetaGo.transform.SetSiblingIndex(i);

            UITarjetaBarraOrdenTurno scTarjeta = tarjetasComponentes[i];
            if (scTarjeta == null)
            {
                continue;
            }

            scTarjeta.Configurar(lUnidades[i], i);
        }
    }

    private void AsegurarCapacidadTarjetas(int cantidadNecesaria)
    {
        while (tarjetasInstanciadas.Count < cantidadNecesaria)
        {
            GameObject goTarjeta = Instantiate(prefabTarjetaBarraOrdenTurno, transform);
            UITarjetaBarraOrdenTurno scTarjeta = goTarjeta.GetComponentInChildren<UITarjetaBarraOrdenTurno>(true);
            if (scTarjeta == null)
            {
                Debug.LogError("UIBarraOrdenTurno: el prefab de tarjeta no contiene UITarjetaBarraOrdenTurno.");
                Destroy(goTarjeta);
                return;
            }

            tarjetasInstanciadas.Add(goTarjeta);
            tarjetasComponentes.Add(scTarjeta);
        }
    }
}
