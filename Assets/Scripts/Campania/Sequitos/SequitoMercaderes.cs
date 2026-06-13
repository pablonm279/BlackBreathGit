using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SequitoMercaderes : MonoBehaviour
{
    private const int RarezaMinTienda = 0; // Comun
    private const int RarezaMaxTienda = 4; // Legendario
    private const int CorrimientoPorFase = 10;

    [Header("Fuente de Items")]
    [SerializeField] ItemDatabase itemDatabase;
    [SerializeField] bool usarItemDatabase = true;

    [SerializeField] List<Arma> ArmasAVender = new List<Arma>();
    [SerializeField] List<Armadura> ArmadurasAVender = new List<Armadura>();
    [SerializeField] List<Accesorio> AccesoriosAVender = new List<Accesorio>();
    [SerializeField] List<Consumible> ConsumiblesAVender = new List<Consumible>();



    [SerializeField] TextMeshProUGUI txtTierTiendas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraTiendas;
    [SerializeField] GameObject btnMejorarTiendas;

    public GameObject prefabBtnItemVendido;
    public Sprite Pin;
    public List<Item> ItemsVendidos = new List<Item>();
    bool restauradoDesdeSave;

    public int intItemsaVender = 5;

    private Item itemPineado;
    private string itemPineadoId = string.Empty;


    public Transform listaItemsVenta;

    void Start()
    {
        AsegurarReferenciaDatabase();
        if (!restauradoDesdeSave && (ItemsVendidos == null || ItemsVendidos.Count == 0))
        {
            GenerarItemsVendidos();
        }
        else
        {
            MostrarInventarioVenta();
        }

    }

    public void MarcarRestauradoDesdeSave()
    {
        restauradoDesdeSave = true;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (itemDatabase == null)
        {
            itemDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDatabase>("Assets/Data/ItemDatabase.asset");
        }
    }
#endif

    void AsegurarReferenciaDatabase()
    {
        if (!usarItemDatabase || itemDatabase != null)
        {
            return;
        }

#if UNITY_EDITOR
        itemDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDatabase>("Assets/Data/ItemDatabase.asset");
#endif

        if (itemDatabase == null)
        {
            Debug.LogWarning("[Items] SequitoMercaderes no tiene ItemDatabase asignada. Se usaran listas legacy.");
        }
    }

    public ItemDatabase GetItemDatabase()
    {
        AsegurarReferenciaDatabase();
        return itemDatabase;
    }

    public void Actualizar()
    {

        //Mejora Tiendas
        int tier = CampaignManager.Instance.sequitoMercaderesTier;
        txtTierTiendas.text = TRADU.i.Traducir("Tamaño Tiendas: ") + (int)tier;
        int valor = 30 + CampaignManager.Instance.sequitoMercaderesTier * 12;
        txtCostoMejoraTiendas.text = valor + TRADU.i.Traducir(" Materiales");


        if (tier > 2)
        {
            btnMejorarTiendas.SetActive(false);
        }
        else
        {
            btnMejorarTiendas.SetActive(true);
        }


        if (CampaignManager.Instance.GetMaterialesActuales() < 30 + (CampaignManager.Instance.sequitoMercaderesTier * 12))
        {
            txtCostoMejoraTiendas.color = Color.red;
        }
        else
        {
            txtCostoMejoraTiendas.color = new Color(40, 40, 0);
        }

    }

    public void MejorarTiendas()
    {
        if (CampaignManager.Instance.GetMaterialesActuales() >= 30 + (CampaignManager.Instance.sequitoMercaderesTier * 12) && CampaignManager.Instance.sequitoMercaderesTier < 3)
        {
            CampaignManager.Instance.CambiarMaterialesActuales(-(30 + (CampaignManager.Instance.sequitoMercaderesTier * 12)));
            CampaignManager.Instance.sequitoMercaderesTier++;
            Actualizar();
        }
    }

    public void GenerarItemsVendidos()
    {
        intItemsaVender = 6 + (3 * CampaignManager.Instance.sequitoMercaderesTier);
        Item itemPineadoActual = itemPineado;
        string itemPineadoIdActual = itemPineadoId;
        bool hayItemPineado = itemPineadoActual != null && itemPineadoActual.gameObject != null;

        if (hayItemPineado)
        {
            ItemsVendidos.Remove(itemPineadoActual);
        }
        // Destruir todas las instancias previas de ItemsVendidos
        foreach (Item item in ItemsVendidos)
        {
            if (item != null)
            {
                Destroy(item.gameObject); // Destruir el GameObject asociado al item
            }
        }

        // Limpiar la lista de ItemsVendidos antes de agregar nuevos elementos
        ItemsVendidos.Clear();

        // Arma pools desde DB (activos y no excluidos de tiendas), con fallback a listas legacy.
        List<Arma> armasDisponibles;
        List<Armadura> armadurasDisponibles;
        List<Accesorio> accesoriosDisponibles;
        List<Consumible> consumiblesDisponibles;

        bool loadedFromDatabase = TryBuildPoolsFromDatabase(
            out armasDisponibles,
            out armadurasDisponibles,
            out accesoriosDisponibles,
            out consumiblesDisponibles);

        if (!loadedFromDatabase)
        {
            armasDisponibles = new List<Arma>(ArmasAVender);
            armadurasDisponibles = new List<Armadura>(ArmadurasAVender);
            accesoriosDisponibles = new List<Accesorio>(AccesoriosAVender);
            consumiblesDisponibles = new List<Consumible>(ConsumiblesAVender);
            RemoverItemsInicialesDeClase(armasDisponibles);
            RemoverItemsInicialesDeClase(armadurasDisponibles);
            RemoverItemsInicialesDeClase(accesoriosDisponibles);
            RemoverItemsInicialesDeClase(consumiblesDisponibles);
        }

        // Determinar cuántos items agregar de cada lista
        if (hayItemPineado)
        {
            EliminarItemPineadoDePools(armasDisponibles, itemPineadoActual, itemPineadoIdActual);
            EliminarItemPineadoDePools(armadurasDisponibles, itemPineadoActual, itemPineadoIdActual);
            EliminarItemPineadoDePools(accesoriosDisponibles, itemPineadoActual, itemPineadoIdActual);
            EliminarItemPineadoDePools(consumiblesDisponibles, itemPineadoActual, itemPineadoIdActual);
        }

        int cantidadObjetivo = intItemsaVender - (hayItemPineado ? 1 : 0);
        int cantidadPorLista = cantidadObjetivo / 4;
        int restante = cantidadObjetivo % 4; // Resto en caso de que no sea divisible exactamente

        // Crear una instancia de Random para seleccionar aleatoriamente
        System.Random random = new System.Random();

        // Agregar elementos al azar de cada lista (armas, armaduras, accesorios, consumibles)
        AgregarItemsAlAzar(armasDisponibles, cantidadPorLista, random);
        AgregarItemsAlAzar(armadurasDisponibles, cantidadPorLista, random);
        AgregarItemsAlAzar(accesoriosDisponibles, cantidadPorLista, random);
        AgregarItemsAlAzar(consumiblesDisponibles, cantidadPorLista, random);

        // Manejar el resto
        for (int i = 0; i < restante; i++)
        {
            int listaAleatoria = random.Next(4); // Ahora son 4 listas
            if (listaAleatoria == 0 && armasDisponibles.Count > 0)
            {
                AgregarItemsAlAzar(armasDisponibles, 1, random);
            }
            else if (listaAleatoria == 1 && armadurasDisponibles.Count > 0)
            {
                AgregarItemsAlAzar(armadurasDisponibles, 1, random);
            }
            else if (listaAleatoria == 2 && accesoriosDisponibles.Count > 0)
            {
                AgregarItemsAlAzar(accesoriosDisponibles, 1, random);
            }
            else if (listaAleatoria == 3 && consumiblesDisponibles.Count > 0)
            {
                AgregarItemsAlAzar(consumiblesDisponibles, 1, random);
            }
        }

        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El Séquito de Mercaderes ha actualizado su oferta."));
        CompletarSlotsRestantesConPools(
            armasDisponibles,
            armadurasDisponibles,
            accesoriosDisponibles,
            consumiblesDisponibles,
            random);
        if (hayItemPineado)
        {
            ItemsVendidos.Add(itemPineadoActual);
        }
        MostrarInventarioVenta();
    }

    // Método auxiliar para agregar items al azar
    private void AgregarItemsAlAzar<T>(List<T> itemsDisponibles, int cantidad, System.Random random) where T : Item
    {
        for (int i = 0; i < cantidad; i++)
        {
            if (itemsDisponibles.Count > 0)
            {
                // Filtrar solo los items que pueden ser usados por algún personaje
                List<T> itemsValidos = new List<T>();
                foreach (T item in itemsDisponibles)
                {
                    if (TienePersonajedeLaClasedelItem(item))
                    {
                        itemsValidos.Add(item);
                    }
                }

                if (itemsValidos.Count == 0)
                    break;

                T itemElegido = SeleccionarItemPorRareza(itemsValidos, random);
                if (itemElegido == null)
                {
                    break;
                }

                ItemsVendidos.Add(Instantiate(itemElegido));
                itemsDisponibles.Remove(itemElegido);
            }
        }
    }

    private void CompletarSlotsRestantesConPools(
        List<Arma> armasDisponibles,
        List<Armadura> armadurasDisponibles,
        List<Accesorio> accesoriosDisponibles,
        List<Consumible> consumiblesDisponibles,
        System.Random random)
    {
        if (random == null)
        {
            random = new System.Random();
        }

        while (ItemsVendidos.Count < intItemsaVender)
        {
            List<Item> candidatos = new List<Item>();
            candidatos.AddRange(FiltrarItemsValidos(armasDisponibles));
            candidatos.AddRange(FiltrarItemsValidos(armadurasDisponibles));
            candidatos.AddRange(FiltrarItemsValidos(accesoriosDisponibles));
            candidatos.AddRange(FiltrarItemsValidos(consumiblesDisponibles));

            if (candidatos.Count == 0)
            {
                break;
            }

            Item elegido = SeleccionarItemPorRareza(candidatos, random);
            if (elegido == null)
            {
                break;
            }

            ItemsVendidos.Add(Instantiate(elegido));

            if (elegido is Arma arma)
            {
                armasDisponibles.Remove(arma);
            }
            else if (elegido is Armadura armadura)
            {
                armadurasDisponibles.Remove(armadura);
            }
            else if (elegido is Accesorio accesorio)
            {
                accesoriosDisponibles.Remove(accesorio);
            }
            else if (elegido is Consumible consumible)
            {
                consumiblesDisponibles.Remove(consumible);
            }
        }
    }

    private List<T> FiltrarItemsValidos<T>(List<T> source) where T : Item
    {
        List<T> result = new List<T>();
        if (source == null)
        {
            return result;
        }

        for (int i = 0; i < source.Count; i++)
        {
            T item = source[i];
            if (item != null && TienePersonajedeLaClasedelItem(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    private T SeleccionarItemPorRareza<T>(List<T> candidatos, System.Random random) where T : Item
    {
        if (candidatos == null || candidatos.Count == 0 || random == null)
        {
            return null;
        }

        List<T> elegibles = new List<T>();
        for (int i = 0; i < candidatos.Count; i++)
        {
            T item = candidatos[i];
            if (item == null)
            {
                continue;
            }

            if (item.iRareza >= RarezaMinTienda && item.iRareza <= RarezaMaxTienda)
            {
                elegibles.Add(item);
            }
        }

        if (elegibles.Count == 0)
        {
            return null; // 0% mitico/artefacto en tienda
        }

        int rarezaObjetivo = TirarRarezaObjetivoMercaderes(random);

        for (int rareza = rarezaObjetivo; rareza >= RarezaMinTienda; rareza--)
        {
            T item = ElegirAleatorioPorRareza(elegibles, rareza, random);
            if (item != null)
            {
                return item;
            }
        }

        for (int rareza = rarezaObjetivo + 1; rareza <= RarezaMaxTienda; rareza++)
        {
            T item = ElegirAleatorioPorRareza(elegibles, rareza, random);
            if (item != null)
            {
                return item;
            }
        }

        return elegibles[random.Next(elegibles.Count)];
    }

    private T ElegirAleatorioPorRareza<T>(List<T> candidatos, int rareza, System.Random random) where T : Item
    {
        List<T> pool = new List<T>();
        for (int i = 0; i < candidatos.Count; i++)
        {
            T item = candidatos[i];
            if (item != null && item.iRareza == rareza)
            {
                pool.Add(item);
            }
        }

        if (pool.Count == 0)
        {
            return null;
        }

        return pool[random.Next(pool.Count)];
    }

    private int TirarRarezaObjetivoMercaderes(System.Random random)
    {
        int fase = ObtenerFaseActualMapa();
        int corrimiento = Mathf.Clamp(fase - 1, 0, 2) * CorrimientoPorFase; // fase 2: +10, fase 3: +20
        int tiradaBase = random.Next(1, 101);
        int tiradaAjustada = Mathf.Clamp(tiradaBase + corrimiento, 1, 100);

        if (tiradaAjustada <= 35) return 0; // Comun 35%
        if (tiradaAjustada <= 65) return 1; // Infrecuente 30%
        if (tiradaAjustada <= 85) return 2; // Raro 20%
        if (tiradaAjustada <= 95) return 3; // Epico 10%
        return 4; // Legendario 5%
    }

    private int ObtenerFaseActualMapa()
    {
        if (CampaignManager.Instance == null || CampaignManager.Instance.scAtributosZona == null)
        {
            return 1;
        }

        return Mathf.Max(1, CampaignManager.Instance.scAtributosZona.FASE);
    }

    bool TienePersonajedeLaClasedelItem(Item item)
    {
        if (item == null)
        {
            return false;
        }

        // Si no tiene clases configuradas o incluye -1, cualquier clase puede usarlo.
        if (item.UsaTodasLasClases())
        {
            return true;
        }

        // Evita ofrecer items que ninguna clase actual del grupo pueda equipar/usar.
        if (CampaignManager.Instance == null || CampaignManager.Instance.scMenuPersonajes == null || CampaignManager.Instance.scMenuPersonajes.listaPersonajes == null)
        {
            return false;
        }

        foreach (Personaje personaje in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
        {
            if (personaje == null)
            {
                continue;
            }

            if (item.PuedeUsarClase(personaje.IDClase))
            {
                return true;
            }
        }

        return false;

    }

    public void MostrarInventarioVenta()
    {

        foreach (Transform transform in listaItemsVenta)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
            Destroy(transform.gameObject);
        }


        foreach (Item goItem in ItemsVendidos)
        {

            GameObject btnItem = Instantiate(prefabBtnItemVendido, listaItemsVenta);
            btnItemEnVenta scBtnItem = btnItem.GetComponent<btnItemEnVenta>();

            scBtnItem.imageMuestraItem.sprite = goItem.GetComponent<Item>().imItem;
            scBtnItem.itemRepresentado = goItem.GetComponent<Item>();
            scBtnItem.ConfigurarPin(Pin, EsItemPineado(goItem.GetComponent<Item>()));



        }



    }

    public bool IntentarPinearItem(Item item)
    {
        if (item == null)
        {
            return false;
        }

        if (EsItemPineado(item))
        {
            DespinearItem(item);
            MostrarInventarioVenta();
            return false;
        }

        itemPineado = item;
        itemPineadoId = ObtenerIdItem(item);
        MostrarInventarioVenta();
        return true;
    }

    public void DespinearItem(Item item)
    {
        if (item == null)
        {
            return;
        }

        if (!EsItemPineado(item))
        {
            return;
        }

        itemPineado = null;
        itemPineadoId = string.Empty;
    }

    public bool EsItemPineado(Item item)
    {
        if (item == null || itemPineado == null)
        {
            return false;
        }

        if (item == itemPineado)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(itemPineadoId))
        {
            string currentId = ObtenerIdItem(item);
            if (!string.IsNullOrWhiteSpace(currentId) && currentId == itemPineadoId)
            {
                return true;
            }
        }

        return ItemsSonEquivalentes(item, itemPineado);
    }

    private string ObtenerIdItem(Item item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        ItemDatabase database = GetItemDatabase();
        string id = ItemSaveCatalog.ResolveItemId(item, database);
        return id ?? string.Empty;
    }

    private void EliminarItemPineadoDePools<T>(List<T> pool, Item itemPineadoActual, string itemPineadoIdActual) where T : Item
    {
        if (pool == null || itemPineadoActual == null)
        {
            return;
        }

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            T candidato = pool[i];
            if (candidato == null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(itemPineadoIdActual))
            {
                string candidatoId = ObtenerIdItem(candidato);
                if (!string.IsNullOrWhiteSpace(candidatoId) && candidatoId == itemPineadoIdActual)
                {
                    pool.RemoveAt(i);
                    continue;
                }
            }

            if (ItemsSonEquivalentes(candidato, itemPineadoActual))
            {
                pool.RemoveAt(i);
            }
        }
    }

    private bool ItemsSonEquivalentes(Item a, Item b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return a.GetType() == b.GetType()
            && a.sNombreItem == b.sNombreItem
            && a.iPrecio == b.iPrecio
            && a.iRareza == b.iRareza
            && a.nivelMejora == b.nivelMejora
            && a.IDEfectoEspecial == b.IDEfectoEspecial;
    }

    private bool EsItemInicialDeClase(Item item)
    {
        if (item == null || CampaignManager.Instance == null || CampaignManager.Instance.scContprefab == null)
        {
            return false;
        }

        ContenedorPrefabsCamp contenedor = CampaignManager.Instance.scContprefab;
        return item == contenedor.armaMandoble
            || item == contenedor.Coraza
            || item == contenedor.armaArcoLargo
            || item == contenedor.ArmaduraCuero
            || item == contenedor.armaBaculoPurificador
            || item == contenedor.armaEspadaCorta
            || item == contenedor.ArmaduraCueroReforzado
            || item == contenedor.armaEstoque
            || item == contenedor.ArmaduraGambeson;
    }

    private void RemoverItemsInicialesDeClase<T>(List<T> items) where T : Item
    {
        if (items == null)
        {
            return;
        }

        items.RemoveAll(item => EsItemInicialDeClase(item));
    }

    bool TryBuildPoolsFromDatabase(
        out List<Arma> armas,
        out List<Armadura> armaduras,
        out List<Accesorio> accesorios,
        out List<Consumible> consumibles)
    {
        armas = new List<Arma>();
        armaduras = new List<Armadura>();
        accesorios = new List<Accesorio>();
        consumibles = new List<Consumible>();

        if (!usarItemDatabase || itemDatabase == null || itemDatabase.items == null)
        {
            return false;
        }

        HashSet<Item> uniqueItems = new HashSet<Item>();
        for (int i = 0; i < itemDatabase.items.Count; i++)
        {
            ItemDatabaseEntry entry = itemDatabase.items[i];
            if (!EsEntradaValidaParaTiendas(entry))
            {
                continue;
            }

            Item prefab = entry.prefab;
            if (prefab == null || EsItemInicialDeClase(prefab) || !uniqueItems.Add(prefab))
            {
                continue;
            }

            if (prefab is Arma arma)
            {
                armas.Add(arma);
            }
            else if (prefab is Armadura armadura)
            {
                armaduras.Add(armadura);
            }
            else if (prefab is Accesorio accesorio)
            {
                accesorios.Add(accesorio);
            }
            else if (prefab is Consumible consumible)
            {
                consumibles.Add(consumible);
            }
        }

        return armas.Count + armaduras.Count + accesorios.Count + consumibles.Count > 0;
    }

    List<Item> BuildItemsListFromDatabase()
    {
        List<Item> result = new List<Item>();
        if (!usarItemDatabase || itemDatabase == null || itemDatabase.items == null)
        {
            return result;
        }

        HashSet<Item> uniqueItems = new HashSet<Item>();
        for (int i = 0; i < itemDatabase.items.Count; i++)
        {
            ItemDatabaseEntry entry = itemDatabase.items[i];
            if (!EsEntradaValidaParaTiendas(entry))
            {
                continue;
            }

            if (entry.prefab != null && !EsItemInicialDeClase(entry.prefab) && uniqueItems.Add(entry.prefab))
            {
                result.Add(entry.prefab);
            }
        }

        return result;
    }

    bool EsEntradaValidaParaTiendas(ItemDatabaseEntry entry)
    {
        return entry != null
            && entry.prefab != null
            && entry.activo
            && !entry.excluirDeTiendas;
    }

    public Item ObtenerItemAlAzar()
    {
        // Usa DB cuando existe: activos y no excluidos de tiendas.
        List<Item> todosLosItems = BuildItemsListFromDatabase();

        // Fallback legacy
        if (todosLosItems.Count == 0)
        {
            todosLosItems.AddRange(ArmasAVender);
            todosLosItems.AddRange(ArmadurasAVender);
            todosLosItems.AddRange(AccesoriosAVender);
            todosLosItems.AddRange(ConsumiblesAVender);
            todosLosItems.RemoveAll(item => EsItemInicialDeClase(item));
        }

        if (todosLosItems.Count == 0)
            return null;

        System.Random random = new System.Random();
        int index = random.Next(todosLosItems.Count);
        return todosLosItems[index];
    }
   
}



