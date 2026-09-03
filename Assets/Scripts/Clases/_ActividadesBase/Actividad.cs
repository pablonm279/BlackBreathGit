using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actividad : MonoBehaviour
{
   
   public int IDActividad;
   //1 - Descansar
   //2 - Entrenar
   //3 - Guardia
   //4 - Caballero: Relatos de Batalla
   //5 - Caballero: Mantenimiento Armadura
   //6 - Caballero: Vigilar
   //7 - Cazador: Caza
   //8 - Cazador: Preparar Flechas
   //9 - Cazador: Exploración
   //10 - Purificadora: Ritual de Limpieza
   //11 - Purificadora: Ayudar a los Desamparados
   //12 - Purificadora: Colaborar con los Curanderos
   //13 - Acechador: Afilar Arma
   //14 - Acechador: Vigilar Desde Las Sombras
   //15 - Acechador: Coerción
   //16 - Canalizador: Concentración Arcana
   //17 - Canalizador: Telekinesis
   //18 - Canalizador: Crear Símbolo Arcano de Protección
   //19 - Duelista: Siempre Alerta
   //20 - Duelista: Socializar
   //21 - Duelista: Consuelo

















   public string desc = "";

   public virtual string ObtenerDescripcion(Personaje personaje = null)
   {
      string descripcion = TRADU.i != null ? TRADU.i.Traducir(desc) : desc;
      return AplicarColorDescripcionSuperior(descripcion);
   }

   protected string AplicarColorDescripcionSuperior(string descripcion)
   {
      return descripcion.Replace("<color=#d3d3d3><i>", "<color=#a8a8a8><i>");
   }


}



