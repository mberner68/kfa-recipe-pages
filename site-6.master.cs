using System;
using System.Web.UI;
using BlogEngine.Core;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using BlogEngine.Core.Providers;
using RecipeDomain;

public partial class SimpleBlog29 : System.Web.UI.MasterPage
{
    private static Regex reg = new Regex(@"(?<=[^])\t{2,}|(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,11}(?=[<])|(?=[\n])\s{2,}");

    protected static string ShRoot = Utils.ApplicationRelativeWebRoot + "editors/tiny_mce_3_5_8/plugins/syntaxhighlighter/";
    public StringBuilder parentNavBuild = new StringBuilder();
    public string strParentID = string.Empty;
    public string strFirstParentID = string.Empty;
    public string pageHeading = string.Empty;
    public string orphanPage = string.Empty;
    protected void Page_Load(object sender, EventArgs e)
    {
        //var pageNavItems = BlogService.FillPages().Where(p => p.IsDeleted == false && !p.NavOrder.Equals(string.Empty)).OrderBy(x => x.NavOrder).ToList();
        //foreach (var ogNAv2 in BlogEngine.Core.Page.Pages.Where(p => p.NavOrder.Equals(string.Empty)).OrderBy(x => x.NavOrder))
        //{
        //    if (Request.Url.ToString().ToLower().Contains(ogNAv2.Id.ToString().Replace("{", "").Replace("}", "")))
        //    {
        //        orphanPage = "home-page";
        //    }
        //}


        if (NavHelper.Instance.IsOphanPage(Request))
        {
            orphanPage = "home-page";
        }

        Page.Header.DataBind();
        pageHeading = Page.Header.Title;
        pageHeading = NavHelper.Instance.GetRootLevelTitle(Page.Header.Title); 

        if (!IsPostBack)
        {
            bool autoSearch = false;

            DataAccess da = new DataAccess();
            RecipeCategory.DataSource = da.GetCategoryRecipeList();
            RecipeCategory.DataTextField = "Name";
            RecipeCategory.DataValueField = "Id";
            RecipeCategory.AppendDataBoundItems = true;
            RecipeCategory.DataBind();

            if (Request.QueryString["cat"] != null)
            {
                try
                {
                    autoSearch = true;
                    RecipeCategory.SelectedValue = Request.QueryString["cat"];

                }
                catch (Exception ex)
                { }
            }
            if (Request.QueryString["key"] != null)
            {
                autoSearch = true;
                RecipeKeywords.Text = Request.QueryString["key"];
            }

            var m = ReadSearchModel();
            var x = SearchRecipies(m);


            if (autoSearch)
            {
                WriteSearchModel(x);
                Jumper.Text = @"<script>$(document).ready(function () { window.location.href = '#res'; });</script>";
            }
            else
            {
                resultsPanel.Visible = false;
                Jumper.Text = @"<script></script>";
            }
        }
        else
        {
            resultsPanel.Visible = true;
            Jumper.Text = @"<script>$(document).ready(function () { window.location.href = '#res'; });</script>";
        }
    }

    private Guid HasFirstLevelParent(Guid guid)
    {
        var firstLevelParetnGuid = new Guid();
        foreach (var ogNav in BlogEngine.Core.Page.Pages.Where(p => p.Id.Equals(guid)))
        {
            if (ogNav.HasParentPage)
            { firstLevelParetnGuid = ogNav.Parent; }
        }
        return firstLevelParetnGuid;
    }

    protected override void Render(HtmlTextWriter writer)
    {
        using (HtmlTextWriter htmlwriter = new HtmlTextWriter(new System.IO.StringWriter()))
        {
            base.Render(htmlwriter);
            writer.Write(htmlwriter.InnerWriter.ToString());
        }
    }

    private RecipeSearchResultModel SearchRecipies(RecipeSearchModel model)
    {
        DataAccess da = new DataAccess();
        return da.SearchRecipes(model);
    }

    private void WriteSearchModel(RecipeSearchResultModel model)
    {

        ResultsRepeater.DataSource = model.Items;
        ResultsRepeater.DataBind();

        NextBtn.Visible = model.NextPage.HasValue;
        PreviousBtn.Visible = model.PrevPage.HasValue;
       
        CurrentPage.Value = model.Page.ToString();
        PageDisplay.Text = model.Page.ToString();
        MaxPageDisplay.Text = model.MaxPages.ToString();
        MaxPerPage.Value = model.ResultCount.ToString();

        if(model.MaxResultCount > 0)
        {

            int a = (int)model.MaxResultCount;
            int end = model.ResultEndIndex > a ? a : model.ResultEndIndex;
            ResultsOfDisplay.Text = string.Format("({0}-{1} of {2})", model.ResultStartIndex, end, model.MaxResultCount);
        }
        else
        {
            ResultsOfDisplay.Text = "0 results";
        }
    }

    private RecipeSearchModel ReadSearchModel()
    {
        RecipeSearchModel model = new RecipeSearchModel();
        
        try
        {
            if (RecipeCategory.SelectedValue != "0")
                model.CategoryId = Convert.ToInt32(RecipeCategory.SelectedValue);
        }
        catch (Exception ex)
        { }

        model.ItemsPerPage = GetItemsPerPage();
        model.Page = GetCurrentPage();
        model.Keywords = RecipeKeywords.Text;
        try
        {
            model.Page = Convert.ToDouble(CurrentPage.Value);
        }
        catch (Exception ex)
        { }

        model.SortBy = RecipeSortByAdded.Checked ? RecipeSearchModel.SortOrder.RecentlyAdded : RecipeSearchModel.SortOrder.TopRated;
        model.WithoutIngredients = RecipeWithoutIngredients.Text;

        List<int> al = new List<int>();
        
        if (ChkEgg.Checked)
            al.Add(4);
        if (ChkFish.Checked)
            al.Add(5);
        if (ChkGluten.Checked)
            al.Add(8);
        if (ChkMilk.Checked)
            al.Add(10);
        if (ChkPeanut.Checked)
            al.Add(15);
        if (ChkSesame.Checked)
            al.Add(20);
        if (ChkShell.Checked)
            al.Add(21);
        if (ChkSoy.Checked)
            al.Add(22);
        if (ChkTree.Checked)
            al.Add(26);
        if (ChkWheat.Checked)
            al.Add(27);

        if (al.Count > 0)
            model.AllerganFreeOfList = al.ToArray();

        return model;
    }

    private double GetItemsPerPage()
    {
        int i = 10;
        try
        {
            i = Convert.ToInt32(MaxPerPage.Value);
        }
        catch (Exception ex)
        { }
        return i;
    }
    private double GetCurrentPage()
    {
        int i = 1;
        try
        {
            i = Convert.ToInt32(CurrentPage.Value);
        }
        catch (Exception ex)
        { }
        return i;
    }

    protected void submit2_Click(object sender, EventArgs e)
    {
        var m = ReadSearchModel();
        m.Page = 1;
        var x = SearchRecipies(m);
        WriteSearchModel(x);
    }
  /*  protected void ItemsPerPage5_Click(object sender, EventArgs e)
    {
        var m = ReadSearchModel();
        m.ItemsPerPage = 5;
        m.Page = 1;
        var x = SearchRecipies(m);
        WriteSearchModel(x);
    }*/
    protected void ItemsPerPage10_Click(object sender, EventArgs e)
    {
        var m = ReadSearchModel();
        m.ItemsPerPage = 10;
        m.Page = 1;
        var x = SearchRecipies(m);
        WriteSearchModel(x);
    }
    protected void ItemsPerPage20_Click(object sender, EventArgs e)
    {
        var m = ReadSearchModel();
        m.ItemsPerPage = 20;
        m.Page = 1;
        var x = SearchRecipies(m);
        WriteSearchModel(x);
    }
    protected void ItemsPerPage50_Click(object sender, EventArgs e)
    {
        var m = ReadSearchModel();
        m.ItemsPerPage = 50;
        m.Page = 1;
        var x = SearchRecipies(m);
        WriteSearchModel(x);
    }

    protected void NextBtn_Click(object sender, EventArgs e)
    {
        var m = ReadSearchModel();
        m.Page = m.Page + 1;
        var x = SearchRecipies(m);
        WriteSearchModel(x);
    }
    protected void PreviousBtn_Click(object sender, EventArgs e)
    {
        var m = ReadSearchModel();
        m.Page = m.Page - 1;
        var x = SearchRecipies(m);
        WriteSearchModel(x);
    }
}
