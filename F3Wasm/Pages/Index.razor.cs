using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using F3Wasm;
using Blazorise;
using Blazorise.Components;
using F3Wasm.Models;
using F3Wasm.Data;

namespace F3Wasm.Pages
{
    public partial class Index
    {
        private string comment = string.Empty;
        private List<string> allNames = new List<string>();
        private List<Pax> pax = new List<Pax>();
        private DateTime? qDate = DateTime.Now;
        public string ao { get; set; }

        public string errorMessage { get; set; }
        public bool showCompleteAlert { get; set; }
        public bool isLoading { get; set; }

        private async Task OnCommentButtonClicked()
        {
            allNames = await LambdaHelper.GetPaxNamesAsync(Http);
            allNames = allNames.OrderBy(x => x).ToList();
            pax = PaxHelper.GetPaxFromComment(comment, allNames);

            // No need to show that we finished again, we're doing another aoo
            showCompleteAlert = false;
        }

        private async Task OnUploadButtonClicked()
        {
            // Validate
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(ao))
            {
                errorMessage = "Please select an AO";
                return;
            }

            if (qDate == null || !qDate.HasValue)
            {
                errorMessage = "Please select a Q Date";
                return;
            }

            if (!pax.Any(x => x.IsQ))
            {
                errorMessage = "Please select a Q";
                return;
            }

            if (pax.Any(x => string.IsNullOrEmpty(x.Name)))
            {
                errorMessage = "Please Fix the Missing Names";
                return;
            }

            if (pax.GroupBy(x => x.Name).Any(g => g.Count() > 1))
            {
                errorMessage = "Please Fix the Duplicate Names";
                return;
            }

            try
            {
                isLoading = true;
                await LambdaHelper.UploadPaxAsync(Http, pax, ao, qDate.Value);
                showCompleteAlert = true;

                // Reset everything
                comment = string.Empty;
                pax = new List<Pax>();
                qDate = DateTime.Now;
                ao = string.Empty;
                isLoading = false;
            }
            catch (Exception ex)
            {
                errorMessage = "There was an error uploading the data. Please try again." + ex.Message;
            }
        }

        void OnQSelected(Pax member, bool isChecked)
        {
            //pax.ForEach(x => x.IsQ = false);
            if (isChecked)
            {
                member.IsQ = true;
            }
            else
            {
                member.IsQ = false;
            }
        }

        void OnFngSelected(Pax member, bool isChecked)
        {
            if (isChecked)
            {
                member.Name = member.UnknownName;
                member.UnknownName = string.Empty;
                member.IsFng = true;
            }
            else
            {
                member.UnknownName = member.Name;
                member.Name = string.Empty;
                member.IsFng = false;
            }
        }

        void OnRemovePax(Pax member)
        {
            pax.Remove(member);
        }
    }
}