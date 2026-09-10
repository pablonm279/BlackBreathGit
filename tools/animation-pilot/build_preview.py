"""Vista comparativa autocontenida del piloto; no necesita servidor ni Unity."""
import base64
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
TARGET = ROOT / "output/animation-pilot"


def uri(path):
    return "data:image/png;base64," + base64.b64encode(path.read_bytes()).decode()


characters = []
for name, label, folder, source, ground, scale, shift, impact, cadence in [
    ("CaballeroApariencia1", "Caballero · apariencia 1", "Assets/Scripts/Clases/Caballero",
     ["Caballero_idle.png", "Caballero_mueve.png", "Caballero_Ataca.png", "Caballero_idle2.png", "Caballero_habilidad.png", "caballero_posedef.png"], 0.074, 1.365333, 0.05, 0.42, 0.16),
    ("DriadaQuemada", "Dríada quemada", "Assets/Prefabs/Prefabs NPC/ZONA - Bosque de los Lamentos/DriadaQuemada",
     ["driadaidle.png", "driadamueve.png", "driadaataca.png", "driadaactiva.png", "driadahabilidad.png", "driadaactiva.png"], 0.012, 1.365333, -0.01, 0.35, 0.20),
]:
    characters.append(dict(label=label, ground=ground, scale=scale, shift=shift, impact=impact, cadence=cadence,
        aspect=0.924 if name == "CaballeroApariencia1" else 0.842,
        old=[uri(ROOT/folder/f) for f in source],
        frames=[uri(ROOT/"Assets/Resources/AnimacionesIlustradas"/name/(f+".png"))
                for f in ["reposo", "anticipacion", "impacto"]]))

html = '''<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Caballero y dríada · prueba de animación</title>
<style>
*{box-sizing:border-box}body{margin:0;background:#11171e;color:#ece8dd;font:15px system-ui,sans-serif}main{max-width:1200px;margin:auto;padding:32px 24px}h1{font-size:28px;font-weight:600;margin:8px 0}p{color:#a6afb7;line-height:1.6;max-width:820px}.eyebrow{font-size:11px;letter-spacing:2px;color:#ccb17c}nav{display:flex;gap:8px;flex-wrap:wrap;margin:24px 0}button,select{background:#202b35;color:#e9e4d8;border:1px solid #3c4651;border-radius:6px;padding:10px 15px;cursor:pointer;font:inherit}button.active{background:#ccaf77;color:#161b22;border-color:#ccaf77}.grid{display:grid;grid-template-columns:1fr 1fr;gap:20px}.card{border:1px solid #35404a;border-radius:10px;overflow:hidden;background:#19232c}.title{padding:16px 18px;border-bottom:1px solid #35404a;font-weight:600}.labels{display:flex;justify-content:space-around;font-size:11px;letter-spacing:1.5px;color:#adbac4;padding:18px 0 0}canvas{display:block;width:100%;height:auto}.hint{font-size:12px;color:#89949e;margin-top:18px}@media(max-width:760px){.grid{grid-template-columns:1fr}h1{font-size:23px}}
</style><main><div class="eyebrow">SILVERLAND / PRUEBA VISUAL</div><h1>Dos personajes, nuevas secuencias</h1>
<p>Comparación del arte original con los sprites preparados para Unity. Elegí una acción para ver las poses y el ritmo; el combate conserva sus tiempos existentes.</p>
<nav id="actions"><button data-mode="idle" class="active">Reposo</button><button data-mode="alert">Atento</button><button data-mode="move">Dash</button><button data-mode="attack">Ataque</button><button data-mode="damage">Daño</button><button data-mode="skill">Habilidad</button><button data-mode="defense">Postura defensiva</button><button id="pause">Pausar</button><select id="speed" aria-label="Velocidad"><option value="1">Velocidad normal</option><option value="0.5">Mitad de velocidad</option><option value="0.25">Un cuarto de velocidad</option></select></nav>
<div class="grid" id="cards"></div><p class="hint">El dash conserva su sprite y recibe impulso y recuperación suaves. Atento se usa al tener el turno o ser objetivo. El daño conserva la pose, sin sprite propio; aquí se muestra una reacción breve estando atento. Habilidad y postura defensiva conservan su arte original; la dríada no tiene postura defensiva. La iluminación y los efectos del combate se comprueban por separado en Unity.</p></main>
<script>const data=__DATA__;let mode='idle',paused=false,speed=1,elapsed=0,last=0;
const load=url=>new Promise(resolve=>{const image=new Image;image.onload=()=>resolve(image);image.src=url});
const cards=data.map(d=>{let card=document.createElement('div');card.className='card';card.innerHTML=`<div class="title">${d.label}</div><div class="labels"><span>ORIGINAL</span><span>NUEVO</span></div><canvas width="900" height="580"></canvas>`;document.querySelector('#cards').append(card);return {...d,canvas:card.querySelector('canvas')}});
Promise.all(cards.map(async d=>{d.old=await Promise.all(d.old.map(load));d.frames=await Promise.all(d.frames.map(load))})).then(()=>requestAnimationFrame(tick));
function tick(now){if(last&&!paused)elapsed+=(now-last)/1000*speed;last=now;cards.forEach(draw);requestAnimationFrame(tick)}
function draw(d){const c=d.canvas.getContext('2d');c.clearRect(0,0,900,580);let t=elapsed,frame=0,old=0,originalNew=null,stretch=0,shear=0,dash=0;
if(mode==='idle'||mode==='alert'||mode==='defense'){stretch=Math.sin(t*2.1)*.008;shear=Math.sin(t*1.05)*.0025}
if(mode==='alert'){old=3;originalNew=3}
if(mode==='skill'){old=4;originalNew=4}
if(mode==='defense'){old=5;originalNew=5}
if(mode==='move'){t%=1.4;let moving=t<.42,p=Math.min(1,t/.42);dash=16-32*p*p*(3-2*p);old=moving?1:0;if(moving){originalNew=1;stretch=-.009*Math.sin(Math.min(1,t/.24)*Math.PI);shear=-.025*(1-Math.exp(-t/.07))+.004*Math.sin(t*12)*Math.exp(-t/.28)}else{let r=Math.max(0,1-(t-.42)/.18);shear=-.025*r*r*(3-2*r)}}
if(mode==='attack'){t%=2;old=t<(d.impact===.42?1.45:1)?2:3;frame=t<d.impact?1:2;if(t>=d.impact+.18)originalNew=3;shear=t<d.impact?.014*Math.sin(Math.min(1,t/d.impact)*Math.PI*.5):-.022*Math.max(0,1-(t-d.impact)/.22)}
if(mode==='damage'){t%=1.7;old=3;originalNew=3;shear=.024*Math.sin(Math.min(1,t/.24)*Math.PI)}
const w=350,h=w/d.aspect,y=80,x1=45+dash,x2=495+dash;
c.fillStyle='#70808d';c.globalAlpha=.25;for(const x of [x1,x2]){c.beginPath();c.ellipse(x+w*.54,y+h*(1-d.ground)+6,90,9,0,0,Math.PI*2);c.fill()}c.globalAlpha=1;
c.drawImage(d.old[old],x1,y,w,h);
const original=originalNew!==null,foot=.5+(original?d.shift:0),ax=x2+w*foot,ay=y+h*(1-d.ground);c.save();c.translate(ax+(original?0:w*d.shift),ay);c.transform(original?1:d.scale,0,-shear,1+stretch,0,0);c.drawImage(original?d.old[originalNew]:d.frames[frame],-w*foot,-h*(1-d.ground),w,h);c.restore();
c.strokeStyle='#3a4650';c.beginPath();c.moveTo(450,25);c.lineTo(450,555);c.stroke();}
document.querySelectorAll('[data-mode]').forEach(b=>b.onclick=()=>{mode=b.dataset.mode;elapsed=0;document.querySelectorAll('[data-mode]').forEach(x=>x.classList.toggle('active',x===b))});
document.querySelector('#pause').onclick=e=>{paused=!paused;e.target.textContent=paused?'Continuar':'Pausar'};
document.querySelector('#speed').onchange=e=>speed=Number(e.target.value);
</script></html>'''
(TARGET/"comparacion.html").write_text(html.replace("__DATA__", json.dumps(characters, ensure_ascii=False)), encoding="utf-8")
print(TARGET/"comparacion.html")
