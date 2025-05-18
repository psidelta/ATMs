# Postavke i pravila CSV datoteke

## Postavke formata datoteke
- Separator: Zarez (,)
- Prvi redak sadrži nazive stupaca: Da
- Kultura/Format: Hrvatski (hr-HR)

## Obavezni stupci
1. `RB` - Identifikator bankomata (long)
2. `DATUM DEINSTALACIJE UREDAJA (ako je uredaj u meduvremenu deinstaliran)` - Datum deinstalacije (string, može biti prazno)
3. `ULICA` - Ulica (string, obavezno)
4. `KUCNI BROJ` - Kućni broj (string, opcionalno)
5. `GRAD` - Naziv grada (string)
6. `POSTANSKI BROJ` - Poštanski broj (string)
7. `SKRACENA OZNAKA BANKE` - Oznaka banke (string)
8. `UREDAJ U SKLADISTU (DA/NE)` - Status skladištenja (string, "NE" ili prazno za aktivne bankomate)
9. `BANKOMAT KOJI NE ULAZI U MREZU` - Status mreže (string, "NE" ili prazno za bankomate u mreži)
10. `CIJENA NAJMA BEZ PDV-a (u HRK)` - Cijena najma (double)
11. `CIJENA MJESECNOG NAJMA UREDAJA` - Mjesečna cijena najma uređaja (double, može biti NaN)
12. `BROJ UPLATNIH TRANSAKCIJA - GODISNJE` - Godišnji broj uplatnih transakcija za 2019. (double)
13. `BROJ ISPLATNIH TRANSAKCIJA - GODISNJE` - Godišnji broj isplatnih transakcija za 2019. (double)
14. `BROJ OSTALIH TRANSAKCIJA - GODISNJE` - Godišnji broj ostalih transakcija za 2019. (double)
15. `PROCELJE BANKE (DA/NE)` - Oznaka pročelja banke (string, "DA" za potvrdu)
16. `24 SATNA ZONA (DA/NE)` - Oznaka 24-satne zone (string, "DA" za potvrdu)
17. `GODINA PROIZVODNJE` - Godina proizvodnje bankomata (integer)
18. `TIP MODELA` - Tip bankomata (string, "ISPLATNI" za isplatne bankomate)
19. `PONOVNA CERTIFIKACIJA` - Indeks certifikacije (double)

## Pravila obrade podataka

### Obrada adrese
- Nazivi ulica (`ULICA`) se trimaju i pretvaraju u velika slova
- Kućni brojevi (`KUCNI BROJ`) se trimaju i dodaju ulici s razmakom
- Nazivi gradova (`GRAD`) se trimaju i pretvaraju u velika slova
- Poštanski brojevi (`POSTANSKI BROJ`) se trimaju

### Pravila filtriranja bankomata
Bankomat se uključuje u obradu samo ako su ispunjeni SVI sljedeći uvjeti:
1. Nema datuma deinstalacije (`DATUM DEINSTALACIJE UREDAJA (ako je uredaj u meduvremenu deinstaliran)` je prazno)
2. Ima adresu ulice (`ULICA` nije prazno)
3. Nije u skladištu (`UREDAJ U SKLADISTU (DA/NE)` je prazno ili "NE")
4. U mreži je (`BANKOMAT KOJI NE ULAZI U MREZU` je prazno ili "NE")
5. Odgovara uključenim oznakama banke (ako su specificirane)
6. Odgovara uključenim poštanskim brojevima (ako su specificirani)
7. Odgovara uključenim gradovima (ako su specificirani)
8. Nije među isključenim oznakama banke (ako su specificirane)

### Posebni izračuni
1. **Izračun godišnjeg troška**:
   - Ukupno = (`CIJENA NAJMA BEZ PDV-a (u HRK)` × 12) + (`CIJENA MJESECNOG NAJMA UREDAJA` × 12)
   - Ako je `CIJENA MJESECNOG NAJMA UREDAJA` NaN, tretira se kao 0

2. **Broj transakcija**:
   - Ukupno = `BROJ UPLATNIH TRANSAKCIJA - GODISNJE` + `BROJ ISPLATNIH TRANSAKCIJA - GODISNJE` + `BROJ OSTALIH TRANSAKCIJA - GODISNJE`

3. **Starost bankomata**:
   - Izračunava se kao: Trenutna godina - `GODINA PROIZVODNJE`

4. **Klasifikacija tipa bankomata**:
   - Tip "ISPLATNI" se kodira kao 1
   - Svi ostali tipovi se kodiraju kao 0

### Pravila blokiranih bankomata
Bankomat se označava kao blokiran (ne može se ukloniti) ako:
- Zadovoljava sva osnovna pravila filtriranja I
-`PROCELJE BANKE (DA/NE)` = "DA" ILI `24 SATNA ZONA (DA/NE)` = "DA"
