﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using System.Data.Linq;
using System.Security.Cryptography;

namespace BLL
{
    public class Repository
    {
        public static List<Lekarz> GetAllDoctors()
        {

            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var query = from u in ctx.Uzytkowniks.OfType<Lekarz>() select u;

            return query.ToList();
        }

        public static List<Pacjent> GetAllPatients()
        {

            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var query = from u in ctx.Uzytkowniks.OfType<Pacjent>() select u;

            return query.ToList();
        }

        public static List<Pacjent> GetAllDrPacients(int id)
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();
            var query = from c in ctx.Uzytkowniks.OfType<Lekarz>() where c.id == id select c;
            Lekarz lek = query.First();          

            return lek.Pacjents.ToList();
        }

        public static List<Specjalizacja> GetAllSpecjalizations()
        {
            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();

            var query = from c in ctx.Specjalizacjas select c;
            return query.ToList();
        }

       
        public static bool AddNewDoctor(String imie, String nazwisko, String email, String kodPocztowy, String miasto, string nrDomu, decimal pesel, string telefon, string ulica, List<int> idsSpecjalizacja, String login, String password)
        {

            PrzychodniaDataClassesDataContext ctx = new PrzychodniaDataClassesDataContext();


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

            var query = from c in ctx.Specjalizacjas where c.id == id select c;

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
                throw new NoUserException("Nie odnaleziono użytkownika o podanym loginie i haśle");
            }

            return user;
        }
    }
}
