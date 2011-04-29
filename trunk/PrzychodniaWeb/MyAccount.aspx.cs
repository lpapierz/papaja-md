﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using DAL;
using System.Drawing;

public partial class MyAccount : System.Web.UI.Page
{
    protected Uzytkownik user = null;
    protected Lekarz dr = null;
    private String editMessage;
    public String EditMessage { get { return editMessage; } set { editMessage = value; lblEditMessage.Text = value; } }
    private String passwordChangeMessage;
    public String PasswordChangeMessage { get { return passwordChangeMessage; } set { passwordChangeMessage = value; lblPasswordChangeMessage.Text = value; } }

    protected void InitializePanelspec()
    {
        panelSpec.Controls.Clear();

        List<Specjalizacja> specList = Repository.GetAllDrSpecjalizations(dr);

        Table specTable = new Table();

        TableHeaderRow th = new TableHeaderRow();
        TableHeaderCell thc = new TableHeaderCell();
        thc.Text = "Lista specjalizacji:";

        th.Cells.Add(thc);
        specTable.Rows.Add(th);

        foreach (Specjalizacja s in specList)
        {
            TableRow tr = new TableRow();
            TableCell tc = new TableCell();

            tc.Text = s.nazwa;
            tr.Cells.Add(tc);
            specTable.Rows.Add(tr);
   
        }

        panelSpec.Controls.Add(specTable);
    }

    protected void InitializePanelEditSpec()
    {
        try
        {
            panelEditSpec2.Controls.Clear();
            List<Specjalizacja> specList = Repository.GetAllSpecjalizations();

            foreach (Specjalizacja s in specList)
            {
                CheckBox cb = new CheckBox();
                cb.ID = s.id.ToString();
                cb.Text = s.nazwa;
                cb.CssClass = "cbSpec";

                panelEditSpec2.Controls.Add(cb);

                if (dr.Specjalizacja_Lekarzs.FirstOrDefault(sl => sl.idSpecjalizacja == s.id) != null)
                {
                    cb.Checked = true;
                }
            }

        }
        catch (Exception ex)
        {
            Master.Message = ex.Message;
            Master.SetMessageColor(Color.Red);
        }

    }
    
    protected void InitializePanelEdit()
    {
        tbEditCity.Text = dr.miasto;
        tbEditEmail.Text = dr.email;
        tbEditPhone.Text = dr.telefon;
        tbEditPostalCode.Text = dr.kod_pocztowy;
        tbEditStreet.Text = dr.ulica;
        tbEditStreetNr.Text = dr.nr_domu;
        tbPassword.Text = dr.password;
        tbConfPassword.Text = dr.password;
        tbEditLogin.Text = dr.login;
        tbEditSurname.Text = dr.nazwisko;
        tbEditName.Text = dr.imie;

        try
        {
            tbEditPesel.Text = dr.pesel.ToString();
        }
        catch (Exception ex)
        {
            tbEditPesel.Text = "";
        }
        
    }

    protected void InitializeTableHours()
    {
        try
        {
            List<Godziny_przyj> hours = dr.Godziny_przyjs.ToList();

            foreach (Godziny_przyj g in hours)
            {
                switch ( g.dzien )
                {
                    case 1:
                        lblDay1.Text += g.godz_od.ToString().Substring(0, 5) + "-" + g.godz_do.ToString().Substring(0, 5) + ", ";
                        break;
                    case 2:
                        lblDay2.Text += g.godz_od.ToString().Substring(0, 5) + "-" + g.godz_do.ToString().Substring(0, 5) + ", ";
                        break;
                    case 3:
                        lblDay3.Text += g.godz_od.ToString().Substring(0, 5) + "-" + g.godz_do.ToString().Substring(0, 5) + ", ";
                        break;
                    case 4:
                        lblDay4.Text += g.godz_od.ToString().Substring(0, 5) + "-" + g.godz_do.ToString().Substring(0, 5) + ", ";
                        break;
                    case 5:
                        lblDay5.Text += g.godz_od.ToString().Substring(0, 5) + "-" + g.godz_do.ToString().Substring(0, 5) + ", ";
                        break;
                    case 6:
                        lblDay6.Text += g.godz_od.ToString().Substring(0, 5) + "-" + g.godz_do.ToString().Substring(0, 5) + ", ";
                        break;
                    case 7:
                        lblDay7.Text += g.godz_od.ToString().Substring(0, 5) + "-" + g.godz_do.ToString().Substring(0, 5) + ", ";
                        break;
                    default:
                        throw new BadDayInentifyierException();
                }
            }          
        }
        catch (Exception ex)
        {
            Master.Message = ex.Message;
            Master.SetMessageColor(Color.Red);
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!Page.IsPostBack)
            {
                if (Session["userId"] != null)
                {
                    user = Repository.GetUserByID(Int32.Parse(Session["userId"].ToString()));
                    dr = user as Lekarz;

                    String lbl = "Dr @NAME @SURNAME";
                    lbl = lbl.Replace("@NAME", dr.imie);
                    lbl = lbl.Replace("@SURNAME", dr.nazwisko);

                    lblName.Text = lbl;

                    InitializePanelspec();

                    lblPesel.Text = dr.pesel.ToString();
                    lblEmail.Text = dr.email;
                    lblPhone.Text = dr.telefon;
                    lblAdres.Text = "ul. " + dr.ulica + " " + dr.nr_domu + ", " + dr.kod_pocztowy + " " + dr.miasto;

                    InitializePanelEdit();
                    InitializeTableHours();
                    InitializePanelEditSpec();
                }
                else
                {
                    Response.Redirect("~/Login.aspx");
                }
            }
        }
        catch (Exception ex)
        {
            Master.Message = ex.Message;
            Master.SetMessageColor( Color.Red );
        }
    }

    protected void btnSubmitEdit_Click(object sender, EventArgs e)
    {
        try
        {
            dr = Repository.GetUserByID(Int32.Parse(Session["userId"].ToString())) as Lekarz;

            String login = tbEditLogin.Text; 
            String city = tbEditCity.Text;
            String email = tbEditEmail.Text;
            String name = tbEditName.Text;
            Decimal pesel = Decimal.Parse( tbEditPesel.Text );
            String phone =  tbEditPhone.Text;
            String postalCode = tbEditPostalCode.Text;
            String street = tbEditStreet.Text;
            String streetNr = tbEditStreetNr.Text;
            String surname = tbEditSurname.Text;

            Repository.UpdateDrData(dr, name, surname, email, postalCode, city, streetNr, pesel, phone, street, null, login);

            EditMessage = "Dane poprawnie zmienione.";
            lblEditMessage.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            EditMessage = ex.Message;
            lblEditMessage.ForeColor = Color.Red;
        }
    }
    protected void btnChangePassword_Click(object sender, EventArgs e)
    {
        try
        {
            dr = Repository.GetUserByID(Int32.Parse(Session["userId"].ToString())) as Lekarz;


            String password = tbPassword.Text;
            String confPassword = tbConfPassword.Text;

            if (password != confPassword) throw new PasswordDontMatchException();

            Repository.ChangeUserPassword(dr, password);

            PasswordChangeMessage = "Hasło poprawnie zmienione.";
            lblPasswordChangeMessage.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            PasswordChangeMessage = ex.Message;
            lblPasswordChangeMessage.ForeColor = Color.Red;
        }
    }
}