using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDatabaseEntry> items = new List<ItemDatabaseEntry>();

    public ItemDatabaseEntry BuscarPorId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return items.Find(entry => string.Equals(entry.id, id, StringComparison.OrdinalIgnoreCase));
    }

    public List<ItemDatabaseEntry> FiltrarPorCategoria(string categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria))
        {
            return new List<ItemDatabaseEntry>(items);
        }

        return items.FindAll(entry => string.Equals(entry.categoria, categoria, StringComparison.OrdinalIgnoreCase));
    }

    public List<ItemDatabaseEntry> FiltrarPorTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return new List<ItemDatabaseEntry>(items);
        }

        return items.FindAll(entry =>
        {
            if (entry.tags == null)
            {
                return false;
            }

            for (int i = 0; i < entry.tags.Count; i++)
            {
                if (string.Equals(entry.tags[i], tag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        });
    }
}

[Serializable]
public class ItemBuffsData
{
    public int buffFuerza;
    public int buffAgi;
    public int buffPoder;
    public int buffIniciativa;
    public int buffApMax;
    public int buffValMax;
    public int buffhpMax;
    public int buffArmadura;
    public int buffDefensa;
    public int buffTSReflejo;
    public int buffTSFortaleza;
    public int buffTSMental;
    public int buffResFuego;
    public int buffResRayo;
    public int buffResHielo;
    public int buffResArcano;
    public int buffResAcido;
    public int buffResNecro;
    public int buffResDivino;
    public int barreraInicioCombate;
    public int evasionInicioCombate;
    public int bonusDanioFuegoInicioCombate;
    public int bonusDanioHieloInicioCombate;
    public int bonusDanioRayoInicioCombate;
    public int bonusDanioAcidoInicioCombate;
    public int bonusDanioArcanoInicioCombate;
    public int bonusDanioNecroInicioCombate;
    public int bonusDanioDivinoInicioCombate;
    public int regeneracionVidaInicioCombate;
    public int regeneracionArmaduraInicioCombate;
    public int reduccionDanioRecibidoPorcentaje;
    public int reduccionDanioCriticoRecibidoPorcentaje;
    public int resistenciaEstadosPorcentaje;
    public int espinasDanioPlano;
    public int espinasDanioPorcentaje;
}

[Serializable]
public class ItemDatabaseEntry
{
    [Tooltip("Label shown in list as Tipo - Nombre")]
    public string listaTitulo;

    public string id;
    public Item prefab;
    public string nombre;

    [TextArea(2, 6)]
    public string descripcion;

    public string categoria;
    public int rareza;
    public Sprite icono;
    public int precio;
    public int nivelMejora;
    public int idEfectoEspecial;

    public List<int> clasesPermitidas = new List<int>();

    public int requisitoFue;
    public int requisitoAgi;
    public int requisitoPoder;

    public ItemBuffsData buffs = new ItemBuffsData();
    public List<DebuffImpactoArmaData> debuffsImpactoArma = new List<DebuffImpactoArmaData>();

    public Habilidad habilidadAtaque;
    public Habilidad habilidadExtra1;
    public Habilidad habilidadExtra2;

    public List<string> tags = new List<string>();
    public bool activo = true;
    public bool excluirDeTiendas = false;
}
