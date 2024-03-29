﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using System.Data.Linq;
using System.Security.Cryptography;
using System.Globalization;
using System.Data;
using System.Reflection;

namespace BLL
{
    public class Repository
    {
        public static List<Lekarz> GetAllDoctors()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var query = from u in ctx.Uzytkowniks.OfType<Lekarz>()
                        select u;

            return query.ToList();
        }


        public static List<Pacjent> GetAllPatients()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var query = from u in ctx.Uzytkowniks.OfType<Pacjent>()
                        select u;

            return query.ToList();
        }

        public static List<Pacjent> GetAllDrPatients(int id)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var query = from c in ctx.Uzytkowniks.OfType<Lekarz>()
                        where c.id == id
                        select c;
            Lekarz lek = query.First();

            return lek.Pacjents.ToList();
        }

        public static List<Specjalizacja> GetAllSpecjalizations()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var query = from c in ctx.Specjalizacjas
                        select c;
            return query.ToList();
        }

        public static void DeleteUser(int userId)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Uzytkownik user = ctx.Uzytkowniks.SingleOrDefault(u => u.id == userId);

            //gdy lekarz, edytuj dane jego pacjentów
            if (user is Lekarz)
            {
                foreach (Pacjent p in user.Pacjents)
                {
                    p.id_lek = null;
                }
            }

            ctx.Uzytkowniks.DeleteOnSubmit(user);

            ctx.SubmitChanges();

        }


        public static bool AddNewDoctor(String imie, String nazwisko, String email, String kodPocztowy, String miasto, string nrDomu, decimal pesel, string telefon, string ulica, List<int> idsSpecjalizacja, String login, String password)
        {

            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var query = from u in ctx.Uzytkowniks
                        where u.login == login
                        select u;

            Uzytkownik user = query.SingleOrDefault();

            if (user != null)
            {
                throw new UserExistException();
            }

            String passSHA1 = CalculateSHA1(password, Encoding.ASCII);

            Lekarz l = new Lekarz()
            {
                imie = imie,
                nazwisko = nazwisko,
                email = email,
                kod_pocztowy = kodPocztowy,
                miasto = miasto,
                nr_domu = nrDomu,
                pesel = pesel,
                telefon = telefon,
                ulica = ulica,
                login = login,
                password = passSHA1

            };

            List<Specjalizacja_Lekarz> list = new List<Specjalizacja_Lekarz>();
            foreach (int i in idsSpecjalizacja)
            {
                Specjalizacja_Lekarz s = new Specjalizacja_Lekarz()
                {
                    idSpecjalizacja = i
                };
                list.Add(s);

            }

            l.Specjalizacja_Lekarzs.AddRange(list);

            ctx.Uzytkowniks.InsertOnSubmit(l);
            ctx.SubmitChanges();

            return true;
        }

        public static bool AddNewPatient(String imie, String nazwisko, String kodPocztowy, String miasto, string nrDomu, decimal? pesel, string telefon, string ulica, int? idDr)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Pacjent p = new Pacjent()
            {
                imie = imie,
                nazwisko = nazwisko,
                kod_pocztowy = kodPocztowy,
                miasto = miasto,
                nr_domu = nrDomu,
                pesel = pesel,
                telefon = telefon,
                ulica = ulica
            };

            p.id_lek = idDr;

            ctx.Uzytkowniks.InsertOnSubmit(p);
            ctx.SubmitChanges();

            return true;
        }

        public static bool AddNewSpecialization(string name)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Specjalizacja s = new Specjalizacja()
            {
                nazwa = name
            };

            ctx.Specjalizacjas.InsertOnSubmit(s);
            ctx.SubmitChanges();

            return true;
        }

        public static Specjalizacja GetSpecializationById(int id)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var query = from c in ctx.Specjalizacjas
                        where c.id == id
                        select c;

            return query.First();
        }

        public static string CalculateSHA1(string text, Encoding enc)
        {
            byte[] buffer = enc.GetBytes(text);
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).ToLower().Replace("-", "");
            return hash;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="pswd"></param>
        /// <returns></returns>
        /// <exception cref="NoUserException">NoUserException</exception>
        public static Uzytkownik UserAuth(string login, string pswd)
        {
            String pass = CalculateSHA1(pswd, Encoding.ASCII);
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var query = from u in ctx.Uzytkowniks
                        where u.login == login && u.password == pass
                        select u;

            Uzytkownik user = query.FirstOrDefault();

            if (user == null)
            {
                throw new NoUserException();
            }

            return user;
        }

        public static Uzytkownik GetUserByID(int id)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var query = from u in ctx.Uzytkowniks
                        where u.id == id
                        select u;

            return query.SingleOrDefault();
        }

        public static List<Specjalizacja> GetAllDrSpecjalizations(Lekarz dr)
        {
            List<Specjalizacja> specList = new List<Specjalizacja>();

            foreach (Specjalizacja_Lekarz sl in dr.Specjalizacja_Lekarzs)
            {
                specList.Add(Repository.GetSpecializationById(sl.idSpecjalizacja));
            }

            return specList;
        }

        public static void UpdateUserData(Uzytkownik user, String imie, String nazwisko, String kodPocztowy, String miasto, string nrDomu, Decimal? pesel, string telefon, string ulica, String login)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            if (!String.IsNullOrEmpty(login))
            {

                var query = from u in ctx.Uzytkowniks
                            where u.login == login && u.id != user.id
                            select u;

                Uzytkownik us = query.SingleOrDefault();

                if (us != null)
                {
                    throw new UserExistException();
                }

            }

            var query2 = from u in ctx.Uzytkowniks
                         where u.id == user.id
                         select u;
            Uzytkownik usr = query2.SingleOrDefault();

            usr.imie = imie;
            usr.nazwisko = nazwisko;
            usr.kod_pocztowy = kodPocztowy;
            usr.miasto = miasto;
            usr.nr_domu = nrDomu;
            usr.telefon = telefon;
            usr.ulica = ulica;
            usr.login = login;
            usr.pesel = pesel;

            ctx.SubmitChanges();
        }

        public static void UpdateUserData(Uzytkownik u)
        {
            Repository.UpdateUserData(u, u.imie, u.nazwisko, u.kod_pocztowy,
                u.miasto, u.nr_domu, u.pesel, u.telefon, u.ulica, u.login);
        }

        public static void UpdateDrData(Lekarz dr, String imie, String nazwisko, String email, String kodPocztowy, String miasto, string nrDomu, Decimal? pesel, string telefon, string ulica, List<int> idsSpecjalizacja, String login)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            Repository.UpdateUserData(dr, imie, nazwisko, kodPocztowy, miasto, nrDomu, pesel, telefon, ulica, login);

            var query = from u in ctx.Uzytkowniks.OfType<Lekarz>()
                        where u.id == dr.id
                        select u;
            Lekarz usr = query.SingleOrDefault();

            usr.email = email;
            ctx.SubmitChanges();
        }

        public static void UpdateDrData(Lekarz dr)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            Repository.UpdateUserData(dr, dr.imie, dr.nazwisko, dr.kod_pocztowy,
                dr.miasto, dr.nr_domu, dr.pesel, dr.telefon, dr.ulica, dr.login);

            var query = from u in ctx.Uzytkowniks.OfType<Lekarz>()
                        where u.id == dr.id
                        select u;
            Lekarz usr = query.SingleOrDefault();
            usr.email = dr.email;

            ctx.SubmitChanges();
        }

        public static void UpdateAdminData(Administrator admin, String name, String surname, String email, String postalCode, String city, String streetNr, Decimal? pesel, String phone, String street, String login)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            Repository.UpdateUserData(admin, name, surname, postalCode, city, streetNr, pesel, phone, street, login);

            var query = from u in ctx.Uzytkowniks.OfType<Administrator>()
                        where u.id == admin.id
                        select u;
            Administrator usr = query.SingleOrDefault();

            usr.email = email;
            ctx.SubmitChanges();
        }

        public static void UpdatePatjentData(Pacjent patjent, String name, String surname, String postalCode, String city, String streetNr, Decimal? pesel, String phone, String street, String ubezpieczenie, int id_lek)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            Repository.UpdateUserData(patjent, name, surname, postalCode, city, streetNr, pesel, phone, street, null);

            var query = from u in ctx.Uzytkowniks.OfType<Pacjent>()
                        where u.id == patjent.id
                        select u;
            Pacjent usr = query.SingleOrDefault();

            usr.id_lek = id_lek;
            usr.ubezpieczenie = ubezpieczenie;
            ctx.SubmitChanges();
        }

        public static void UpdatePatjentData(Pacjent patjent)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            Repository.UpdateUserData(patjent, patjent.imie, patjent.nazwisko, patjent.kod_pocztowy,
                patjent.miasto, patjent.nr_domu, patjent.pesel, patjent.telefon, patjent.ulica, null);

            var query = from u in ctx.Uzytkowniks.OfType<Pacjent>()
                        where u.id == patjent.id
                        select u;
            Pacjent usr = query.SingleOrDefault();
            usr.ubezpieczenie = patjent.ubezpieczenie;
            usr.id_lek = patjent.id_lek;

            ctx.SubmitChanges();
        }

        public static void ChangeUserPassword(Uzytkownik user, String password)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            String pass = CalculateSHA1(password, Encoding.ASCII);

            var query = from u in ctx.Uzytkowniks
                        where u.id == user.id
                        select u;
            Uzytkownik usr = query.SingleOrDefault();

            usr.password = pass;
            ctx.SubmitChanges();
        }

        /// <summary>
        /// Funkcja dodaje specjalizacje dla podanego lekarza
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="idSpec"></param>
        public static void AddSpecjalization(Lekarz dr, int idSpec)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            Specjalizacja_Lekarz sp = new Specjalizacja_Lekarz();
            sp.idSpecjalizacja = idSpec;
            sp.idUzytkownik = dr.id;

            //check, if dh already have those spec
            var x = from s in ctx.Specjalizacja_Lekarzs
                    where s.idUzytkownik == dr.id && s.idSpecjalizacja == idSpec
                    select s;

            if (x.FirstOrDefault() != null)
            {
                return;
            }

            ctx.Specjalizacja_Lekarzs.InsertOnSubmit(sp);
            ctx.SubmitChanges();
        }

        /// <summary>
        /// Funkcja usuwa wszystkie specjalizacje podanego lekarza
        /// </summary>
        /// <param name="dr"></param>
        public static void RemoveAllDrSpecjalizations(Lekarz dr)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var x = from sp in ctx.Specjalizacja_Lekarzs
                    where sp.idUzytkownik == dr.id
                    select sp;
            ctx.Specjalizacja_Lekarzs.DeleteAllOnSubmit(x.ToList());
            ctx.SubmitChanges();
        }


        public static void AddNewField(int idPatient, int idCode, String w, String r, String s, String z)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Pacjent patient = ctx.Uzytkowniks.OfType<Pacjent>().SingleOrDefault(p => p.id == idPatient);

            if (patient == null)
                throw new PatientNotExistException();

            Kod_jednostki code = ctx.Kod_jednostkis.SingleOrDefault(c => c.id == idCode);

            if (code == null)
                throw new CodeNotExistException();

            Wpis_kartoteka wk = new Wpis_kartoteka();

            wk.wywiad_badania = w;
            wk.recetpy = r;
            wk.skierowania = s;
            wk.zalecenie = z;

            wk.id_pacj = idPatient;
            wk.id_kod_jedn = idCode;

            wk.data = DateTime.Now;

            patient.ostatnia_wizyta = DateTime.Now;

            ctx.Wpis_kartotekas.InsertOnSubmit(wk);
            ctx.SubmitChanges();
        }

        public static List<Wpis_kartoteka> GetAllPatientFields(int idPatient)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Pacjent patient = ctx.Uzytkowniks.SingleOrDefault(u => u.id == idPatient) as Pacjent;

            if (patient == null)
                throw new PatientNotExistException();

            List<Wpis_kartoteka> wk = ctx.Wpis_kartotekas.Where(w => w.id_pacj == idPatient).OrderByDescending(w => w.data).Select(w => w).ToList();

            return wk;
        }

        public static Wpis_kartoteka GetPatientsFile(int id)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            Wpis_kartoteka wk = ctx.Wpis_kartotekas.SingleOrDefault( w => w.id == id);

            if (wk == null) throw new NoFileExistException();

            return wk;
        }

        [System.ComponentModel.DataObjectMethod(System.ComponentModel.DataObjectMethodType.Update, true)]
        public static void UpdatePatiensField(Wpis_kartoteka wk)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Wpis_kartoteka wkOriginal = ctx.Wpis_kartotekas.SingleOrDefault( w => w.id == wk.id);

            wkOriginal.data = wk.data;
            wkOriginal.skierowania = wk.skierowania;
            wkOriginal.wywiad_badania = wk.wywiad_badania;
            wkOriginal.zalecenie = wk.zalecenie;
            wkOriginal.recetpy = wk.recetpy;

            wkOriginal.id_kod_jedn = wk.id_kod_jedn;

            ctx.SubmitChanges();
        }

        [System.ComponentModel.DataObjectMethodAttribute(System.ComponentModel.DataObjectMethodType.Delete, true)]
        public static void DeletePatientsField(Int32 id)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Wpis_kartoteka wk = ctx.Wpis_kartotekas.SingleOrDefault(w => w.id == id);
            ctx.Wpis_kartotekas.DeleteOnSubmit(wk);
            ctx.SubmitChanges();
        }


        [System.ComponentModel.DataObjectMethodAttribute(System.ComponentModel.DataObjectMethodType.Delete, true)]
        public static void DeletePatientsField(Wpis_kartoteka wk)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Wpis_kartoteka w = ctx.Wpis_kartotekas.SingleOrDefault(ww => ww.id == wk.id);
            ctx.Wpis_kartotekas.DeleteOnSubmit(w);
            ctx.SubmitChanges();
        }

        public static List<Kod_jednostki_grupa> GetAllKJG()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            return ctx.Kod_jednostki_grupas.ToList();
        }

        public static List<Kod_jednostki_podgrupa> GetAllKJPG()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            return ctx.Kod_jednostki_podgrupas.ToList();
        }

        public static List<Kod_jednostki_podgrupa> GetAllKJPG(int gId)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var x = from k in ctx.Kod_jednostki_podgrupas
                    where k.id_grupa == gId
                    select k;
            return x.ToList();
        }

        public static List<Kod_jednostki> GetAllKJ()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            return ctx.Kod_jednostkis.ToList();
        }

        public static List<Kod_jednostki> GetAllKJ(int pgId)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var x = from k in ctx.Kod_jednostkis where k.id_podgrupa == pgId select k;
            return x.ToList();
        }

        public static int GegMaxKJid()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            return (from x in ctx.Kod_jednostkis
                    select x.id).Max();
        }

        public static int GegMaxKJGid()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            return (from x in ctx.Kod_jednostki_grupas
                    select x.id).Max();
        }

        public static int GegMaxKJPGid()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            return (from x in ctx.Kod_jednostki_podgrupas
                    select x.id).Max();
        }

        /// <summary>
        /// Funkcja wyszukuje pacjenta po pesel-u
        /// </summary>
        /// <param name="pesel"></param>
        public static Pacjent GetPatientByPesel(decimal pesel)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var query = from p in ctx.Uzytkowniks.OfType<Pacjent>()
                        where p.pesel == pesel
                        select p;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Funkcje zwracają pierwszy dzień tygodnia
        /// </summary>
        /// <param name="dayInWeek"></param>
        public static DateTime GetFirstDayOfWeek(DateTime dayInWeek)
        {
            CultureInfo defaultCultureInfo = CultureInfo.CurrentCulture;
            return GetFirstDayOfWeek(dayInWeek, defaultCultureInfo);
        }

        public static DateTime GetFirstDayOfWeek(DateTime dayInWeek, CultureInfo cultureInfo)
        {
            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            DateTime firstDayInWeek = dayInWeek.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);

            return firstDayInWeek;
        }

        public static Kod_jednostki GetKJById(int id)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            return ctx.Kod_jednostkis.SingleOrDefault( kj=> kj.id == id ); 
        }

        public static List<Typ_rejestracja> GetAllTyp_rejestracjas()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var query = from c in ctx.Typ_rejestracjas
                        select c;
            return query.ToList();
        }

        /// <summary>
        /// Funkcja dodaje nową rezerwację terminu (rejestrację)
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="dateBegin"></param>
        /// <param name="dateEnd"></param>
        /// <param name="resTypeId"></param>
        public static bool AddNewReservation(int patientId, DateTime dateBegin, DateTime dateEnd, int resTypeId)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Rejestracja rejestracja = new Rejestracja
            {
                id_pacj = patientId,
                data_od = dateBegin,
                data_do = dateEnd,
                id_typ = resTypeId
            };

            ctx.Rejestracjas.InsertOnSubmit(rejestracja);
            ctx.SubmitChanges();

            return true;
        }

        /// <summary>
        /// Funkcja  uaktualnia dane specializacji w bazie.
        /// </summary>
        /// <param name="spec"></param>
        public static void UpdateSpecializationData(Specjalizacja spec)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            
            var query = from s in ctx.Specjalizacjas
                        where s.id == spec.id
                        select s;

            Specjalizacja ss = query.SingleOrDefault();
            ss.nazwa = spec.nazwa;

            ctx.SubmitChanges();
        }

        public static void DeleteSpecialization(int id)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Specjalizacja user = ctx.Specjalizacjas.SingleOrDefault(s => s.id == id);

            ctx.Specjalizacjas.DeleteOnSubmit(user);

            ctx.SubmitChanges();
        }

        public static List<Rejestracja> GetAllPatientReservations(int patientId)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var rej = ctx.Rejestracjas.Where(r => r.id_pacj == patientId).OrderByDescending(r => r.data_od);

            return rej.ToList();
        }

        public static Typ_rejestracja GetTypRejestracjaById(int typId)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var typ = ctx.Typ_rejestracjas.SingleOrDefault(t => t.id == typId);

            return typ;
        }

        public static void DeleteReservation(int resId)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Rejestracja res = ctx.Rejestracjas.SingleOrDefault(r => r.id == resId);

            ctx.Rejestracjas.DeleteOnSubmit(res);

            ctx.SubmitChanges();
        }

        public static List<Dzien> GetAllDays()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var days = ctx.Dziens.Select(d => d).OrderBy(d => d.id);

            return days.ToList();
        }

        public static bool AddNewHours(int lekId, int idDay, string beginHour, string endHour)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            Lekarz lek = ctx.Uzytkowniks.SingleOrDefault(l => l.id == lekId) as Lekarz;

            //1900-01-01 zostaje doklejone, bo tak sa pierwsze rekordy w bazie, wynik jakijs konwersji
            beginHour = "1900-01-01 " + beginHour;
            endHour = "1900-01-01 " + endHour;
            DateTime begin = Convert.ToDateTime(beginHour);
            DateTime end = Convert.ToDateTime(endHour);
            
            Godziny_przyj godzina = new Godziny_przyj
            {
                id_uzytkownik = lekId,
                dzien = idDay,
                godz_od = begin,
                godz_do = end
            };

            lek.Godziny_przyjs.Add(godzina);

            ctx.SubmitChanges();

            return true;
        }

        public static void UpdateHours(int godzinaId, string beginHour, string endHour)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            PrzychodniaDataClassesDataContext ctx2 = new PrzychodniaDataClassesDataContext();

            var godz = ctx.Godziny_przyjs.SingleOrDefault(g => g.id == godzinaId);
            Lekarz lek = ctx2.Uzytkowniks.SingleOrDefault(l => l.id == godz.id_uzytkownik) as Lekarz;

            //1900-01-01 zostaje doklejone, bo tak sa pierwsze rekordy w bazie, wynik jakijs konwersji
            beginHour = "1900-01-01 " + beginHour;
            endHour = "1900-01-01 " + endHour;
            DateTime begin = Convert.ToDateTime(beginHour);
            DateTime end = Convert.ToDateTime(endHour);

            if (godz.godz_od < begin || godz.godz_do > end)
            {
                foreach (Pacjent p in lek.Pacjents)
                {
                    foreach (Rejestracja r in p.Rejestracjas)
                    {
                        if (godz.dzien == ConvertDayOfWeek(r.data_od) 
                            && r.data_od > DateTime.Now)
                        {
                            if (r.data_od < begin || r.data_do > end)
                                DeleteReservation(r.id);
                        }
                    }
                }
            }

            godz.godz_od = begin;
            godz.godz_do = end;

            ctx.SubmitChanges();
        }

        public static int ConvertDayOfWeek(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                case DayOfWeek.Sunday:
                    return 7;
                default:
                    throw new BadDayIdentifyierException();
            }
        }

        public static void DeleteHours(int godzinaId)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            PrzychodniaDataClassesDataContext ctx2 = new PrzychodniaDataClassesDataContext();

            var godz = ctx.Godziny_przyjs.SingleOrDefault(g => g.id == godzinaId);
            Lekarz lek = ctx2.Uzytkowniks.SingleOrDefault(l => l.id == godz.id_uzytkownik) as Lekarz;

            //1900-01-01 zostaje doklejone, bo tak sa pierwsze rekordy w bazie, wynik jakijs konwersji

            foreach (Pacjent p in lek.Pacjents)
            {
                foreach (Rejestracja r in p.Rejestracjas)
                {
                    if (godz.dzien == ConvertDayOfWeek(r.data_od)
                        && r.data_od > DateTime.Now)
                    {
                        if (r.data_od < godz.godz_od || r.data_do > godz.godz_do)
                            DeleteReservation(r.id);
                    }
                }
            }

            ctx.Godziny_przyjs.DeleteOnSubmit(godz);

            ctx.SubmitChanges();
        }
    }
}
