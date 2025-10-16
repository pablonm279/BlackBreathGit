using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Armadura  : Item
{
    
    

    
    public int requisitoFue;
    public int requisitoAgi;
    public int requisitoPoder;
   
    public Habilidad habilidadExtra1;
    public Habilidad habilidadExtra2;


    //Buffs
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


    public void AgregarStatsArmaduraaDescripcion()
    {
        itemDescripcion += "\n\n"; 
        if (TRADU.i.nIdioma == 1)
        {   
            // Fuerza
            if (buffFuerza != 0)
                itemDescripcion += $"\nFuerza: {(buffFuerza > 0 ? "+" : "")}{buffFuerza}";
            // Agilidad
            if (buffAgi != 0)
                itemDescripcion += $"\nAgilidad: {(buffAgi > 0 ? "+" : "")}{buffAgi}";
            // Poder
            if (buffPoder != 0)
                itemDescripcion += $"\nPoder: {(buffPoder > 0 ? "+" : "")}{buffPoder}";
            // Iniciativa
            if (buffIniciativa != 0)
                itemDescripcion += $"\nIniciativa: {(buffIniciativa > 0 ? "+" : "")}{buffIniciativa}";
            // AP Máx
            if (buffApMax != 0)
                itemDescripcion += $"\nAP Máx: {(buffApMax > 0 ? "+" : "")}{buffApMax}";
            // Valor Máx
            if (buffValMax != 0)
                itemDescripcion += $"\nValor Máx: {(buffValMax > 0 ? "+" : "")}{buffValMax}";
            // HP Máx
            if (buffhpMax != 0)
                itemDescripcion += $"\nHP Máx: {(buffhpMax > 0 ? "+" : "")}{buffhpMax}";
            // Armadura
            if (buffArmadura != 0)
                itemDescripcion += $"\nArmadura: {(buffArmadura > 0 ? "+" : "")}{buffArmadura}";
            // Defensa
            if (buffDefensa != 0)
                itemDescripcion += $"\nDefensa: {(buffDefensa > 0 ? "+" : "")}{buffDefensa}";
            // TS Reflejo
            if (buffTSReflejo != 0)
                itemDescripcion += $"\nTS Reflejo: {(buffTSReflejo > 0 ? "+" : "")}{buffTSReflejo}";
            // TS Fortaleza
            if (buffTSFortaleza != 0)
                itemDescripcion += $"\nTS Fortaleza: {(buffTSFortaleza > 0 ? "+" : "")}{buffTSFortaleza}";
            // TS Mental
            if (buffTSMental != 0)
                itemDescripcion += $"\nTS Mental: {(buffTSMental > 0 ? "+" : "")}{buffTSMental}";
            // Resistencias
            if (buffResFuego != 0)
                itemDescripcion += $"\nRes. Fuego: {(buffResFuego > 0 ? "+" : "")}{buffResFuego}";
            if (buffResRayo != 0)
                itemDescripcion += $"\nRes. Rayo: {(buffResRayo > 0 ? "+" : "")}{buffResRayo}";
            if (buffResHielo != 0)
                itemDescripcion += $"\nRes. Hielo: {(buffResHielo > 0 ? "+" : "")}{buffResHielo}";
            if (buffResArcano != 0)
                itemDescripcion += $"\nRes. Arcano: {(buffResArcano > 0 ? "+" : "")}{buffResArcano}";
            if (buffResAcido != 0)
                itemDescripcion += $"\nRes. Ácido: {(buffResAcido > 0 ? "+" : "")}{buffResAcido}";
            if (buffResNecro != 0)
                itemDescripcion += $"\nRes. Necrótico: {(buffResNecro > 0 ? "+" : "")}{buffResNecro}";
            if (buffResDivino != 0)
                itemDescripcion += $"\nRes. Divino: {(buffResDivino > 0 ? "+" : "")}{buffResDivino}";
        }
        if (TRADU.i.nIdioma == 2)
        {
            // Strength
            if (buffFuerza != 0)
                itemDescripcion += $"\nStrength: {(buffFuerza > 0 ? "+" : "")}{buffFuerza}";
            // Agility
            if (buffAgi != 0)
                itemDescripcion += $"\nAgility: {(buffAgi > 0 ? "+" : "")}{buffAgi}";
            // Power
            if (buffPoder != 0)
                itemDescripcion += $"\nPower: {(buffPoder > 0 ? "+" : "")}{buffPoder}";
            // Initiative
            if (buffIniciativa != 0)
                itemDescripcion += $"\nInitiative: {(buffIniciativa > 0 ? "+" : "")}{buffIniciativa}";
            // Max AP
            if (buffApMax != 0)
                itemDescripcion += $"\nMax AP: {(buffApMax > 0 ? "+" : "")}{buffApMax}";
            // Max Valor
            if (buffValMax != 0)
                itemDescripcion += $"\nMax Valor: {(buffValMax > 0 ? "+" : "")}{buffValMax}";
            // Max HP
            if (buffhpMax != 0)
                itemDescripcion += $"\nMax HP: {(buffhpMax > 0 ? "+" : "")}{buffhpMax}";
            // Armor
            if (buffArmadura != 0)
                itemDescripcion += $"\nArmor: {(buffArmadura > 0 ? "+" : "")}{buffArmadura}";
            // Defense
            if (buffDefensa != 0)
                itemDescripcion += $"\nDefense: {(buffDefensa > 0 ? "+" : "")}{buffDefensa}";
            // Reflex Save
            if (buffTSReflejo != 0)
                itemDescripcion += $"\nReflex Save: {(buffTSReflejo > 0 ? "+" : "")}{buffTSReflejo}";
            // Fortitude Save
            if (buffTSFortaleza != 0)
                itemDescripcion += $"\nFortitude Save: {(buffTSFortaleza > 0 ? "+" : "")}{buffTSFortaleza}";
            // Will Save
            if (buffTSMental != 0)
                itemDescripcion += $"\nMental Save: {(buffTSMental > 0 ? "+" : "")}{buffTSMental}";
            // Resistances
            if (buffResFuego != 0)
                itemDescripcion += $"\nFire Res.: {(buffResFuego > 0 ? "+" : "")}{buffResFuego}";
            if (buffResRayo != 0)
                itemDescripcion += $"\nLightning Res.: {(buffResRayo > 0 ? "+" : "")}{buffResRayo}";
            if (buffResHielo != 0)
                itemDescripcion += $"\nIce Res.: {(buffResHielo > 0 ? "+" : "")}{buffResHielo}";
            if (buffResArcano != 0)
                itemDescripcion += $"\nArcane Res.: {(buffResArcano > 0 ? "+" : "")}{buffResArcano}";
            if (buffResAcido != 0)
                itemDescripcion += $"\nAcid Res.: {(buffResAcido > 0 ? "+" : "")}{buffResAcido}";
            if (buffResNecro != 0)
                itemDescripcion += $"\nNecrotic Res.: {(buffResNecro > 0 ? "+" : "")}{buffResNecro}";
            if (buffResDivino != 0)
                itemDescripcion += $"\nDivine Res.: {(buffResDivino > 0 ? "+" : "")}{buffResDivino}";
        }
   }

}
