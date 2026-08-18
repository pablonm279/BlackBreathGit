using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAlmaEndeble : Habilidad
{
   

    
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_AlmaEndeble");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
        if (TRADU.i != null && TRADU.i.nIdioma == 2)
        {
            string debilidad = TerminoDescripcion(TerminoDescripcionId.Debilidad, "Weakness");
            string aflicciones = TerminoDescripcion(TerminoDescripcionId.Afliccion, "Combat Afflictions", "Estado_debuff");
            string alientoNegro = TerminoDescripcion(TerminoDescripcionId.AlientoNegro, "Black Breath");
            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                "Fragile Soul",
                $"{debilidad}: Suffers Afflictions based on {alientoNegro}.",
                new[]
                {
                    LineaDescripcion("Effect", $"{aflicciones} scale with the intensity of {alientoNegro}."),
                    LineaDescripcion("Source", alientoNegro)
                },
                costoSuperior: "",
                colorTitulo: "#cb5000");
            return;
        }

        if (TRADU.i != null && TRADU.i.nIdioma == 3)
        {
            string fraqueza = TerminoDescripcion(TerminoDescripcionId.Debilidad, "Fraqueza");
            string aflicciones = TerminoDescripcion(TerminoDescripcionId.Afliccion, "Aflições de combate", "Estado_debuff");
            string respiroNegro = TerminoDescripcion(TerminoDescripcionId.AlientoNegro, "Respiro Negro");
            txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
                "Alma Frágil",
                $"{fraqueza}: sofre Aflições conforme o {respiroNegro}.",
                new[]
                {
                    LineaDescripcion("Efeito", $"{aflicciones} escalam com a intensidade do {respiroNegro}."),
                    LineaDescripcion("Fonte", respiroNegro)
                },
                costoSuperior: "",
                colorTitulo: "#cb5000");
            return;
        }

        string debilidadEs = TerminoDescripcion(TerminoDescripcionId.Debilidad, "Debilidad");
        string afliccionesEs = TerminoDescripcion(TerminoDescripcionId.Afliccion, "Aflicciones de combate", "Estado_debuff");
        string alientoNegroEs = TerminoDescripcion(TerminoDescripcionId.AlientoNegro, "Aliento Negro");
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
            "Alma Endeble",
            $"{debilidadEs}: sufre Aflicciones según el {alientoNegroEs}.",
            new[]
            {
                LineaDescripcion("Efecto", $"{afliccionesEs} escalan con la intensidad del {alientoNegroEs}."),
                LineaDescripcion("Fuente", alientoNegroEs)
            },
            costoSuperior: "",
            colorTitulo: "#cb5000");
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}




