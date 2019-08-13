import requests
import re
import json

vaziOd = '2019-05-25'
rvGradskiPrigradski = ["rvg", "rvp"]
rvDani = ["Radni_dan", "Subota", "Nedelja"]


#ovde izvlacimo ID svake linije
sveLinije = {}
for tip in rvGradskiPrigradski:
    htmlOdg = requests.get(
        'http://www.gspns.rs/red-voznje/lista-linija?rv=' + tip + '&vaziod=' + vaziOd + '&dan=R')
    nadjeno = re.findall('value="(.*?)"', htmlOdg.text)
    sveLinije[tip] = nadjeno


redVoznjeSveLinije = []
for tip in rvGradskiPrigradski:
    for jednaLinija in sveLinije[tip]:
        
        redVoznjeSveLinije.append({
            'broj': '',
            'naziv': '',
            'smerovi': [],
            'redVoznje': {},
        })

        # u foru ispod se uzima red voznje, za jednu liniju (oba smera) za jedan dan (radni,subota,nedelja)
        for dan in rvDani:
            htmlTabelaZaLiniju = requests.get('http://www.gspns.rs/red-voznje/ispis-polazaka?rv='+tip+'&vaziod='+vaziOd+'&dan='+dan[0]+'&linija%5B%5D='+jednaLinija)
            nazivLinije = re.findall('Linija:(.*)',htmlTabelaZaLiniju.text)[0].strip()
            naziviSmerova = re.findall('<th>(.*?)</th>', htmlTabelaZaLiniju.text)
            redoviVoznjePoSmeruHTML = re.findall('<td.*?>(.*?)</td>',htmlTabelaZaLiniju.text, flags=re.DOTALL)[:-1]
            brojLinije = nazivLinije.split(" ")[0]

            #samo jednom postavim podatke linije (kad prvi put izvrsim ovaj kod)
            if dan == rvDani[0]:
                redVoznjeSveLinije[-1]['broj'] = brojLinije
                redVoznjeSveLinije[-1]['naziv'] = nazivLinije
                redVoznjeSveLinije[-1]['smerovi'] = [smer.split(':')[1].strip() for smer in naziviSmerova]


            #red voznje za smer A i smer B se parsiraju da bi se izvukli sati i minuti polazaka
            for redVoznjeHTML, smerKod in zip(redoviVoznjePoSmeruHTML, ['A','B']):
                poSatimaHTML = redVoznjeHTML.split('<br/>')
                polasci = []
                for jedanSatHTML in poSatimaHTML:
                    if not "<b>" in jedanSatHTML:
                        continue
                    sat = re.findall('<b>(\d\d)</b>', jedanSatHTML)[0]
                    minuti = re.findall('<span.*?>(\d\d).*?</span>', jedanSatHTML)

                    for minut in minuti:
                        polasci.append(sat + ":" + minut)

                redVoznjeSveLinije[-1]['redVoznje'][dan + "_" + smerKod] = polasci

        print("Gotova linija " + redVoznjeSveLinije[-1]['broj'])

with open('jgspns.json', 'wb') as fajl:
    jsonDump = json.dumps(redVoznjeSveLinije, ensure_ascii=False, sort_keys=True)
    jsonDump = jsonDump.replace('&Scaron;', 'Š')
    fajl.write(jsonDump.encode('utf-8'))