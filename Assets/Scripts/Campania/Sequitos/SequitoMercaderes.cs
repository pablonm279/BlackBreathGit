using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoMercaderes : MonoBehaviour
{

    [SerializeField] List<Arma> ArmasAVender = new List<Arma>();
    [SerializeField] List<Armadura> ArmadurasAVender = new List<Armadura>();
    [SerializeField] List<Accesorio> AccesoriosAVender = new List<Accesorio>();
    [SerializeField] List<Consumible> ConsumiblesAVender = new List<Consumible>();



    [SerializeField] TextMeshProUGUI txtTierTiendas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraTiendas;
    [SerializeField] GameObject btnMejorarTiendas;

    public GameObject prefabBtnItemVendido;
    public List<Item> ItemsVendidos = new List<Item>();

    public int intItemsaVender = 5;


    public Transform listaItemsVenta;

    void Start()
    {
        GenerarItemsVendidos();

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
        intItemsaVender = 5 + (3 * CampaignManager.Instance.sequitoMercaderesTier);
        // Destruir todas las instancias previas de ItemsVendidos
        foreach (Item item in ItemsVendidos)
        {
            Destroy(item.gameObject); // Destruir el GameObject asociado al item
        }

        // Limpiar la lista de ItemsVendidos antes de agregar nuevos elementos
        ItemsVendidos.Clear();

        // Crear copias de las listas originales para modificar sin afectar las originales
        List<Arma> armasDisponibles = new List<Arma>(ArmasAVender);
        List<Armadura> armadurasDisponibles = new List<Armadura>(ArmadurasAVender);
        List<Accesorio> accesoriosDisponibles = new List<Accesorio>(AccesoriosAVender);
        List<Consumible> consumiblesDisponibles = new List<Consumible>(ConsumiblesAVender);

        // Determinar cuántos items agregar de cada lista
        int cantidadPorLista = intItemsaVender / 4;
        int restante = intItemsaVender % 4; // Resto en caso de que no sea divisible exactamente

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

                int index = random.Next(itemsValidos.Count);
                ItemsVendidos.Add(Instantiate(itemsValidos[index]));
                itemsDisponibles.Remove(itemsValidos[index]);
            }
        }
    }

    bool TienePersonajedeLaClasedelItem(Item item)
    {
        bool tiene = false;
        List<int> listaClasesIDPresentes = new List<int>();
        //Determina qué clases están presentes en el equipo del jugador
        // Esto se hace para evitar que el jugador compre un item que no puede usar
        foreach (Personaje personaje in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
        {
            if (!listaClasesIDPresentes.Contains(personaje.IDClase))
            {
                listaClasesIDPresentes.Add(personaje.IDClase);
            }
        }
        // Verificar si el item tiene una clase asociada
        if (item.IDClasesQuePuedenUsarEsteItem != null && item.IDClasesQuePuedenUsarEsteItem.Count > 0)
        {
            foreach (int idClase in item.IDClasesQuePuedenUsarEsteItem)
            {
                if (listaClasesIDPresentes.Contains(idClase))
                {
                    tiene = true;
                    break;
                }
            }
        }
        else
        {
            // Si el item no tiene clases asociadas, se asume que es usable por cualquier clase
            tiene = true;
        }

        return tiene;

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



        }



    }
    

    public Item ObtenerItemAlAzar()
    {
        // Crear una lista temporal con todos los items disponibles
        List<Item> todosLosItems = new List<Item>();
        todosLosItems.AddRange(ArmasAVender);
        todosLosItems.AddRange(ArmadurasAVender);
        todosLosItems.AddRange(AccesoriosAVender);
        todosLosItems.AddRange(ConsumiblesAVender);

        if (todosLosItems.Count == 0)
            return null;

        System.Random random = new System.Random();
        int index = random.Next(todosLosItems.Count);
        return todosLosItems[index];
    }
   
}