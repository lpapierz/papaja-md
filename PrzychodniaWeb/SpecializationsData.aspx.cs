﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using System.Drawing;
using DAL;

public partial class SpecializationsData : System.Web.UI.Page
{
    private Uzytkownik user = null;
    private Administrator admin = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (Session["userId"] != null)
            {
                user = Repository.GetUserByID(Int32.Parse(Session["userId"].ToString()));

                if (user is Administrator)
                {
                    admin = user as Administrator;
                }
                else
                {
                    Response.Redirect("~/Login.aspx");
                }
            }
            else
            {
                Response.Redirect("~/Login.aspx");
            }
        }
        catch (Exception ex)
        {
            Master.Message = ex.Message;
            Master.SetMessageColor(Color.Red);
        }
    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        gridViewSpecializations.DataSourceID = "ldsSpec";
        gridViewSpecializations.DataBind();
    }

    protected void gridViewSpecializations_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                foreach (DataControlFieldCell cell in e.Row.Cells)
                {
                    foreach (Control control in cell.Controls)
                    {
                        ImageButton button = control as ImageButton;
                        if (button != null && button.CommandName == "Delete")

                            button.OnClientClick =
                                    "if (!confirm('Jesteś pewien że chcesz usunąć tę specjalizację?')) return;";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Master.Message = ex.Message;
            Master.SetMessageColor(Color.Red);
        }
    }

    protected void gridViewSpecializations_RowDeleted(object sender, GridViewDeletedEventArgs e)
    {
        try
        {
            if (e.Exception != null)
            {
                Master.Message = e.Exception.Message;
                Master.SetMessageColor(Color.Red);
                e.ExceptionHandled = true;
            }

        }
        catch (Exception ex)
        {
            Master.Message = ex.Message;
            Master.SetMessageColor(Color.Red);
        }
    }

    protected void gridViewSpecializations_RowUpdated(object sender, GridViewUpdatedEventArgs e)
    {
        try
        {
            Specjalizacja s = Repository.GetSpecializationById(Int32.Parse(e.Keys["id"].ToString()));
            Repository.UpdateSpecializationData(s);
            
            if (e.Exception != null)
            {
                Master.Message = e.Exception.Message;
                Master.SetMessageColor(Color.Red);
                e.ExceptionHandled = true;
            }
        }
        catch (Exception ex)
        {
            Master.Message = ex.Message;
            Master.SetMessageColor(Color.Red);
        }
    }

    protected void ldsSpec_Deleting(object sender, LinqDataSourceDeleteEventArgs e)
    {

    }

    protected void ldsSpec_Selecting(object sender, LinqDataSourceSelectEventArgs e)
    {
        e.Result = Repository.GetAllSpecjalizations();
    }

    protected void ldsSpec_Updating(object sender, LinqDataSourceUpdateEventArgs e)
    {
       
    }

    protected void btnAddSpecialization_Click(object sender, EventArgs e)
    {
        try
        {
            string name = tbNewSpecialization.Text;
            Repository.AddNewSpecialization(name);
            tbNewSpecialization.Text = "";

            lblResult.ForeColor = System.Drawing.Color.Green;
            lblResult.Text = "Specjalizacja została poprawnie dodana";
        }
        catch (Exception ex)
        {
            Master.Message = ex.Message;
            Master.SetMessageColor(Color.Red);
        }
    }
}