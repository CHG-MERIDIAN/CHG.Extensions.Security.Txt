# CHG.Extensions.Security.Txt

[![Build status](https://ci.appveyor.com/api/projects/status/852syrpnhad01ghq?svg=true)](https://ci.appveyor.com/project/CHG-MERIDIAN/chg-extensions-security-txt)
[![NuGet Version](http://img.shields.io/nuget/v/CHG.Extensions.Security.Txt.svg?style=flat)](https://www.nuget.org/packages/CHG.Extensions.Security.Txt/) [![License](https://img.shields.io/badge/license-APACHE-blue.svg)](LICENSE)

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=CHG-MERIDIAN_CHG.Extensions.Security.Txt&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=CHG-MERIDIAN_CHG.Extensions.Security.Txt)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=CHG-MERIDIAN_CHG.Extensions.Security.Txt&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=CHG-MERIDIAN_CHG.Extensions.Security.Txt)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=CHG-MERIDIAN_CHG.Extensions.Security.Txt&metric=security_rating)](https://sonarcloud.io/dashboard?id=CHG-MERIDIAN_CHG.Extensions.Security.Txt)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=CHG-MERIDIAN_CHG.Extensions.Security.Txt&metric=bugs)](https://sonarcloud.io/dashboard?id=CHG-MERIDIAN_CHG.Extensions.Security.Txt)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=CHG-MERIDIAN_CHG.Extensions.Security.Txt&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=CHG-MERIDIAN_CHG.Extensions.Security.Txt)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=CHG-MERIDIAN_CHG.Extensions.Security.Txt&metric=coverage)](https://sonarcloud.io/dashboard?id=CHG-MERIDIAN_CHG.Extensions.Security.Txt)

Provides a middleware to represent the "security.txt" for ASP.NET Core applications. Security.txt is a proposed standard which allows websites to define security policies, more details see [https://securitytxt.org/](https://securitytxt.org/).

The security.txt page will be shown under `/.well-known/security.txt`. Optionally, the url `/security.txt` will be redirected to the main url.

## Usage
Install the NuGet package `CHG.Extensions.Security.Txt`.
There are 3 ways the content of the security.txt can be defined.

```CSharp
public void ConfigureServices(IServiceCollection services)
{
   ...
    services.AddSecurityText(builder => ...);
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    ...
    app.UseSecurityText();
    ...
    app.UseMvc();
}
```

### Using a pre-defined text file

You can use a predefined text file which is deployed with your application.

```CSharp
public void ConfigureServices(IServiceCollection services)
{
   ...

    services.AddSecurityText(builder =>
       builder.ReadFromFile(Environment.WebRootFileProvider.GetFileInfo("companySecurityinfo.txt"))
    );

    // or via path
     services.AddSecurityText(builder =>
       builder.ReadFromFile("./content/companySecurityinfo.txt"))
    );
   ...
}
```

### Using values from configuration file

It's also possible to read the fields from the configuration.

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddSecurityText(builder =>
       builder.ReadFromConfiguration(Configuration.GetSection("SecurityText"))
    );
}
```

The json configuration file should look like:

```json
{
  "SecurityText": {
    "Introduction": "The ACME Security information.",
    "Contact": "mailto:security@example.com;tel:+1-201-555-0123",
    "Encryption": "https://example.com/pgp-key.txt",
    "Signature": "https://example.com/.well-known/security.txt.sig",
    "Policy": "https://example.com/security-policy.html",
    "Acknowledgments": "https://example.com/hall-of-fame.html",
    "Hiring": "https://example.com/jobs.html",
    "Permission": "none",
    "ValidateValues": true
  }
}

```

### Using code to define values

The fields can also be set via code:

```CSharp
public void ConfigureServices(IServiceCollection services)
{
...
    services.AddSecurityText(builder => { builder
        .SetContact("mailto:security@example.com")
        .SetPolicy("https://example.com/security-policy.html");
    });
}
```

### Validation

Per default all values (except when given via file) will be validated. The validation can be disabled either via the `ValidateValues` configuration key when using config file or via the `DisableValidation` method.

## Code of Conduct
This project and everyone participating in it is governed by the [CHG-MERIDIAN Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to developers@chg-meridian.com.
