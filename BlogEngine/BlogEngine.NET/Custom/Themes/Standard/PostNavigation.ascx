<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" Inherits="BlogEngine.Core.Web.Controls.PostNavigationBase" %>
<div id="postnavigation" class="navigation-posts well-global clearfix">
    <div class="text-left next-post">
        <% if (!string.IsNullOrEmpty(NextPostUrl))
           { %>
        <a href="<%=NextPostUrl %>" class="nav-next text-center">
            <i class="fas fa-chevron-circle-left"></i><br /><span><%=NextPostTitle %></span>
        </a>
        <% } %>
    </div>
    <div class="text-right prev-post ">
        <% if (!string.IsNullOrEmpty(PreviousPostUrl))
           { %>
        <a href="<%=PreviousPostUrl %>" class="nav-prev text-center">
            <i class="fas fa-chevron-circle-right"></i><br /><span><%=PreviousPostTitle %></span>
        <% } %>
    </div>
</div>
