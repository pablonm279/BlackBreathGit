using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInfoChar : MonoBehaviour
{
  [SerializeField] private Image ImagenFondo; 
  [SerializeField] private Slider barraVida;
  [SerializeField] private Image Retrato; 
  [SerializeField] private TextMeshProUGUI vNombre;
  [SerializeField] private TextMeshProUGUI vHP;
  [SerializeField] private TextMeshProUGUI vHPMax;
  [SerializeField] private TextMeshProUGUI vDefensa;
  [SerializeField] private TextMeshProUGUI vArmadura;
  [SerializeField] private TextMeshProUGUI vReflejos;
  [SerializeField] private TextMeshProUGUI vFortaleza;
  [SerializeField] private TextMeshProUGUI vMental;
  [SerializeField] private TextMeshProUGUI vResFue;
  [SerializeField] private TextMeshProUGUI vResHie;
  [SerializeField] private TextMeshProUGUI vResRay;
  [SerializeField] private TextMeshProUGUI vResAci;
  [SerializeField] private TextMeshProUGUI vResArca;
  [SerializeField] private TextMeshProUGUI vResNecro;
  [SerializeField] private TextMeshProUGUI vResDivino;
  [SerializeField] private TextMeshProUGUI vMerito;
  [SerializeField] private GameObject vValentiaContenedor;
  
  [SerializeField] private GameObject vDescenemigoGO;
  [SerializeField] private TextMeshProUGUI vDescEnemigo;
  
  public GameObject contenedorCasillasEstados;
  public GameObject casillaEstadoPrefab;

  public GameObject infoEnemigos;
  public GameObject btninfoEnemigos;


  

  public bool hayUnidadSeleccionadaParaInfo;
  public Unidad unidadMostrada;
  public void ActualizarInfoChar(Unidad scUnidadMostrada)
  {
    if(unidadMostrada != null){unidadMostrada.Marcar(0);}
    if(scUnidadMostrada != null)
    {
     
     unidadMostrada = scUnidadMostrada;
     //ActualizarColoresFondo();
     gameObject.SetActive(true);

    
     BotonSalir.SetActive(hayUnidadSeleccionadaParaInfo);
    
   vNombre.text = scUnidadMostrada.uNombre;
   vHP.text = ((int)scUnidadMostrada.HP_actual) + "/";
   vHPMax.text = ((int)scUnidadMostrada.mod_maxHP) + "";
   vDefensa.text = ((int)scUnidadMostrada.ObtenerdefensaActual()) + "";
   vArmadura.text = ((int)scUnidadMostrada.ObtenerArmaduraActual()) + "";
   vReflejos.text = ((int)scUnidadMostrada.mod_TSReflejos) + "";
   vFortaleza.text = ((int)scUnidadMostrada.mod_TSFortaleza) + "";
   vMental.text = ((int)scUnidadMostrada.mod_TSMental) + "";
   vResFue.text = ((int)scUnidadMostrada.ObtenerResistenciaA(1)) + "";
   vResHie.text = ((int)scUnidadMostrada.ObtenerResistenciaA(2)) + "";
   vResRay.text = ((int)scUnidadMostrada.ObtenerResistenciaA(3)) + "";
   vResAci.text = ((int)scUnidadMostrada.ObtenerResistenciaA(4)) + "";
   vResArca.text = ((int)scUnidadMostrada.ObtenerResistenciaA(5)) + "";
   vResNecro.text = ((int)scUnidadMostrada.ObtenerResistenciaA(6)) + "";
   vResDivino.text = ((int)scUnidadMostrada.ObtenerResistenciaA(7)) + "";

     if(scUnidadMostrada.gameObject.GetComponent<IAUnidad>() != null)
     {
        vValentiaContenedor.SetActive(false);
        vDescEnemigo.text = ActualizarDescripcionAI(); 
        vDescenemigoGO.SetActive(mostrardesc);
        btninfoEnemigos.SetActive(true);
     }
     else
     { 
        vMerito.text = ""+scUnidadMostrada.ValentiaP_actual;
        vValentiaContenedor.SetActive(true);
        vDescenemigoGO.SetActive(false);
        btninfoEnemigos.SetActive(false);
       
     }
     
     barraVida.value = scUnidadMostrada.HP_actual / scUnidadMostrada.mod_maxHP;

     Retrato.sprite = scUnidadMostrada.uRetrato;

     //Estados
     foreach (Transform buttonEstado in contenedorCasillasEstados.transform)//Esto remueve los retratos anteriores antes de recalcular que retratos corresponden
     {
            Destroy(buttonEstado.gameObject);
     }

     if(scUnidadMostrada.estado_ardiendo > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(1,scUnidadMostrada.estado_ardiendo);
     }
     if(scUnidadMostrada.estado_aturdido > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(2,scUnidadMostrada.estado_aturdido);
     }
     if(scUnidadMostrada.estado_acido > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(3,scUnidadMostrada.estado_acido);
     }
     if(scUnidadMostrada.estado_congelado > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(4,scUnidadMostrada.estado_congelado);
     }
     if(scUnidadMostrada.estado_ResistenciasReducidas > 0)
     {
       GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
       GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(5,scUnidadMostrada.estado_ResistenciasReducidas);
     }
     if(scUnidadMostrada.estado_armaduraModificador > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(6,scUnidadMostrada.estado_armaduraModificador);
     }
     if(scUnidadMostrada.estado_sangrado > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(8,scUnidadMostrada.estado_sangrado);
     }
     if(scUnidadMostrada.estado_veneno > 0)
     {  GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(9,scUnidadMostrada.estado_veneno);
     }
     if(scUnidadMostrada.estado_APModificador > 0)
     {
        /*GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(7,scUnidadMostrada.estado_APModificador); */
     }
     if(scUnidadMostrada.estado_regeneravida > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(10,scUnidadMostrada.estado_regeneravida);
     }
     if(scUnidadMostrada.estado_regeneraarmadura > 0)
     {
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(11,scUnidadMostrada.estado_regeneraarmadura);
     }
      if(scUnidadMostrada.estado_evasion > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(12,scUnidadMostrada.estado_evasion);
     }
    
     if (scUnidadMostrada is ClaseExplorador)
     {
            ClaseExplorador exp = (ClaseExplorador)scUnidadMostrada;
            GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(13, exp.Cantidad_flechas);
     }
     if(scUnidadMostrada.bonusdam_acido > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(14,scUnidadMostrada.bonusdam_acido);
     }
       if(scUnidadMostrada.bonusdam_arcano > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(15,scUnidadMostrada.bonusdam_arcano);
     }
       if(scUnidadMostrada.bonusdam_fuego > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(16,scUnidadMostrada.bonusdam_fuego);
     }
       if(scUnidadMostrada.bonusdam_hielo > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(17,scUnidadMostrada.bonusdam_hielo);
     }
       if(scUnidadMostrada.bonusdam_necro > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(18,scUnidadMostrada.bonusdam_necro);
     }
       if(scUnidadMostrada.bonusdam_rayo > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(19,scUnidadMostrada.bonusdam_rayo);
     }
      if(scUnidadMostrada is ClasePurificadora)
     { 
        ClasePurificadora exp = (ClasePurificadora)scUnidadMostrada;
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(20,exp.ObtenerFervor());
     }
     if (scUnidadMostrada.bonusdam_divino > 0)
     {
            GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(21, scUnidadMostrada.bonusdam_divino);
     }
      if(scUnidadMostrada.barreraDeDanio > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(22,(int)scUnidadMostrada.barreraDeDanio);
     }
      if(scUnidadMostrada.tejidoCuracMagica > 0)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(23,(int)scUnidadMostrada.tejidoCuracMagica);
     }
     if(scUnidadMostrada.ObtenerEstaEscondido() == 1)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(24,-1);
     }
     if(scUnidadMostrada.ObtenerEstaEscondido() == 2)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(25,-1);
     }
      if(scUnidadMostrada is ClaseCanalizador)
     { 
        ClaseCanalizador exp = (ClaseCanalizador)scUnidadMostrada;
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(26,exp.ObtenerEnergia());
     }
      if(scUnidadMostrada.estado_Corrupto)
     { 
        GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
        GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(27,1);
     }
         //AGREGAR LOS NUEVOS TMB EN UNIDADCANVAS PARA QUE APAREZCAN EN LA BARRA DE VIDA----!! 
           //Y en stacks poner -1 para que no muestre numero en la barra de vida.
           //Y que el parametro desdeBarraVida sea true.
     


     //MostrarBuffs/Debuffs
         foreach (Buff buff in scUnidadMostrada.gameObject.GetComponents<Buff>())
         {
            if (buff.DuracionBuffRondas != 0 && buff.esBuffVisibleUI)
            {
               GameObject buffCuadro = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
               bool buffdebuff = buff.boolfDebufftBuff;

               string sName = "";
               if (buff.DuracionBuffRondas > 0)
               {
                  sName = buff.buffNombre + " " + buff.DuracionBuffRondas;
               }
               if (buff.DuracionBuffRondas < 0)
               {
                  sName = buff.buffNombre;
               }

               buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarBuff(buff);
            }
         }

       //MostrarReacciones
      foreach (Reaccion buff in scUnidadMostrada.gameObject.GetComponents<Reaccion>())
      {
         GameObject buffCuadro =  Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarReaccion(buff);

      }
       //MostrarMarcas
      foreach (Marca buff in scUnidadMostrada.gameObject.GetComponents<Marca>())
      {
         GameObject buffCuadro =  Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarMarca(buff);

      }


    }
    else
    { 
        gameObject.SetActive(false);
    }

  }
  
  public GameObject BotonSalir;
  public void botonSalirDeseleccionar()
  {
    hayUnidadSeleccionadaParaInfo = false;
    infoEnemigos.SetActive(false);
    ActualizarInfoChar(BattleManager.Instance.unidadActiva);

  }
  public void ActualizarColoresFondo()
  { 
     if(unidadMostrada != null )
    {   
      if(unidadMostrada.CasillaPosicion != null)
      {
      
      if (unidadMostrada == BattleManager.Instance.unidadActiva)
      {
         // Amarillo muy suave
         ImagenFondo.color = new Color(0.95f, 0.95f, 0.75f, 0.4f);
      }
      else if (unidadMostrada.CasillaPosicion.lado == 1)
      {
         // Rojo muy suave
         ImagenFondo.color = new Color(0.95f, 0.75f, 0.75f, 0.4f);
      }
      else
      {
         // Azul muy suave
         ImagenFondo.color = new Color(0.75f, 0.75f, 0.95f, 0.4f);
      }
      }
    }

  }


  string ActualizarDescripcionAI()
  {
    string desc = "";
     if(unidadMostrada.uNombre == "Lobo Espectral")
     {
         desc = "<i>El Lobo Espectral es un enemigo feroz que se mueve y ataca rápidamente, mientras su destreza animal le brinda una buena defensa.</i>\n\n<color=#199F10>-Posee un mordisco imbuído en fuego que además de dañar, puede hacer arder a sus enemigos.</color>\n<color=#EE0000>-Estadísticas débiles.</color>";
     }
     if(unidadMostrada.uNombre == "Lobo Alfa Espectral")
     {
         desc = "<i>El Lobo Alfa Espectral es el líder de la manada, posee una complexión mas fuerte y resistente que los demás lobos aunque es un poco menos ágil.</i>\n\n<color=#199F10>-Tiene la capacidad de aullar para motivar a los demás lobos.</color>\n<color=#EE0000>-Si queda sólo no podrá motivar a nadie.</color>";
     }
     if(unidadMostrada.uNombre == "Driada Quemada")
     {
         desc = "<i>Antes siervas y cuidadoras del bosque, ahora manifestaciones de venganza y odio en contra de cualquier invasor del Bosque Ardiente.</i>\n\n<color=#199F10>-Puede enredar con raíces ignífugas.\n-Ataque de rango.</color>\n<color=#EE0000>-Relativamente débil.</color>";
     }
     if(unidadMostrada.uNombre == "Espectro del Bosque")
     {
         desc = "<i>El Espectro del Bosque es un alma en pena atrapada entre las cenizas de un bosque calcinado, su ira alimentada por la destrucción que no pudo evitar. Errante y vengativo, ataca a quienes osan cruzar su tierra calcinada.</i>\n\n<color=#199F10>-Inmune a ataques físicos.\n-Puede maldecir con Perdición.</color>\n<color=#EE0000>-Pierde parte de su inmunidad física momentáneamente al atacar.</color>";
     }
     if(unidadMostrada.uNombre == "Fuego Fatuo")
     {
         desc = "<i>Un eco etéreo de las llamas que lo consumieron, danzando entre las cenizas como un recordatorio del desastre. Aunque parece inofensivo, guía a los incautos hacia la perdición, vengando la memoria del bosque caído.</i>\n\n<color=#199F10>-Resistente a ataques físicos.\n-Puede encarnarse en sus enemigos.</color>\n<color=#EE0000>-Tiene poca vida.</color>";
     }
      if(unidadMostrada.uNombre == "Treant Espectral")
     {
         desc = "<i>Con su madera marcada y deformada por el fuego, estos antes pastores de árboles ahora deambulan trayendo muerte a los invasores de su hogar.</i>\n\n<color=#199F10>-Buena armadura que se regenera.\n-Puede enredar al golpear a sus enemigos.</color>\n<color=#EE0000>-Débil al fuego.</color>";
     }
      if(unidadMostrada.uNombre == "Manifestación Arcana")
     {
         desc = "<i>Constituído por pura energía arcana, este ente etéreo defiende al Canalizador que le dio forma.</i>\n\n<color=#199F10>-Resistente a ataques físicos.</color>";
     }
      if(unidadMostrada.uNombre == "Vagranilo")
     {
         desc = "<i>Un ser volador cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audición excepcional.</i>\n\n<color=#199F10>-Evasivo.\n-Puede aturdir.\n-Puede atacar a enemigos escondidos.</color>\n<color=#EE0000>-Débil al daño Divino.</color>";
     }
     if(unidadMostrada.uNombre == "Vagranilo Mayor")
     {
         desc = "<i>Un ser terrible cuasihumano oriundo de las profundidades, no tiene vision pero compensa con una capacidad de audición excepcional.</i>\n\n<color=#199F10>-Chirrido Ensordecedor.\n-Puede atacar a enemigos escondidos.\n-Se cura al morder victimas con Sangre Contaminada.</color>\n<color=#EE0000>-Débil al daño Divino.</color>";
     }
     if(unidadMostrada.uNombre == "Ladrón")
     {
         desc = "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Buena capacidad de Crítico.\n-Arranca escondido.\n-Puede envenenar su arma.</color>\n<color=#EE0000>-Bastante débil.</color>";
     }
     if(unidadMostrada.uNombre == "Rufián con Ballesta")
     {
         desc = "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Puede empujar.</color>";
     }
     if (unidadMostrada.uNombre == "Rufián con Mazo")
     {
         desc = "<i>Este hombre ya era malvado antes, y ahora la situación desesperada ha acentuado su crueldad.</i>\n\n<color=#199F10>-Resistente.\n-Golpes devastadores.\n-Se enfurece.</color>\n<color=#EE0000>-Lento para actuar.</color>";
     }
     if (unidadMostrada.uNombre == "Perro Adiestrado")
     {
         desc = "<i>Un perro adiestrado para la batalla, fiel a su amo y feroz con sus enemigos.</i>\n\n<color=#199F10>-Puede Inmovilizar al morder.</color>\n<color=#EE0000>-Relativamente débil.</color>";
     }
     if (unidadMostrada.uNombre == "Devorador Corrompido")
     {
          desc = "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Puede debilitar.\n-Absorbe vida de Personajes Corruptos.</color>\n<color=#EE0000>-Relativamente débil.</color>";
     }
     if (unidadMostrada.uNombre == "Guerrero Corrompido")
     {
          desc = "<i>Otrora un habitante de las tierras, ahora corrompido por el Aliento Negro, deformado y hambriento.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Fuerte.\n-Golpea en zona.</color>\n<color=#EE0000>-Posee sólo un tipo de ataque.</color>";
     }
     if (unidadMostrada.uNombre == "Alimaña Corrompida")
     {
          desc = "<i>No se logra discernir facilmente que animal fue originalmente, pero ahora es una criatura corrompida y muy nociva.</i>\n\n<color=#A020F0>-Corrupto.</color>\n<color=#199F10>-Largo alcance.\n-Crea Masa Contaminada.</color>\n<color=#EE0000>-Movimiento limitado.</color>";
     }
     


   return desc;
  }
  
  public bool mostrardesc;
  public void BotonInfoenemigos()
  {
    if(infoEnemigos.activeInHierarchy)
    {
      mostrardesc = false;

    }else{  mostrardesc = true;}

    ActualizarInfoChar(unidadMostrada);
  }
}
