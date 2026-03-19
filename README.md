ASP.NET Core Identity est la solution intégrée pour l'authentification et l'autorisation. C'est l'équivalent direct de Spring Security, mais avec une approche un peu différente.
Pour l'authentification :  
ASP.NET Core Identity gère :  

-Création de comptes utilisateurs  
-Login/Logout  
-Hashage des mots de passe (avec des algorithmes robustes par défaut)  
-Gestion des rôles  
-Two-factor authentication (2FA)  // TO DO
-Récupération de mot de passe  
-Confirmation d'email# auth-core-identity  

Pour l'autorisation :  
C# utilise un système basé sur des attributs et des policies
