<h1 align="center"> EZREB </h1> <br>
<p align="center">
    <img alt="Ezreb" title="Ezreb" src="https://i.imgur.com/0mfg1na.jpg" width="450">
</p>


<p align="center">
  GitHub in your pocket. Built with Passion.
</p>

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Feedback](#feedback)
- [Contributors](#contributors)
- [how to-use](#how to-use)
- [Backers](#backers-)
- [Sponsors](#sponsors-)
- [Acknowledgments](#acknowledgments)

## Introduction

[![Build Status](https://img.shields.io/travis/gitpoint/git-point.svg?style=flat-square)](https://travis-ci.org/gitpoint/git-point)
[![Coveralls](https://img.shields.io/coveralls/github/gitpoint/git-point.svg?style=flat-square)](https://coveralls.io/github/gitpoint/git-point)
[![All Contributors](https://img.shields.io/badge/all_contributors-73-orange.svg?style=flat-square)](./CONTRIBUTORS.md)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)
[![Commitizen friendly](https://img.shields.io/badge/commitizen-friendly-brightgreen.svg?style=flat-square)](http://commitizen.github.io/cz-cli/)
[![Gitter chat](https://img.shields.io/badge/chat-on_gitter-008080.svg?style=flat-square)](https://gitter.im/git-point)

 EZREB (tunisian word mean speed up) is a set of tools to help dotnet developers speed up building their application.

**Available On Nuget Packages**

https://www.nuget.org/packages/FluentValidationWithEntityValidation/1.0.0


## Features

A few of the things you can do with <b>EZREB</b>:

* Auto Detect EF Core Entity Constraint ( unique,foreign-key,not-nullable)
* Custom Mapping Your DTO properties
* Ignore Specific property for manual validation
* Use Validator directly(ForeignKey,IsUnique...)
* Add Auto Filter Class (Incoming)


## Feedback

Feel free to send us feedback on [Linkedin](https://www.linkedin.com/in/hamza-b-aa9377106/) or [file an issue](https://github.com/hamzabouissi/EZREBissues/new). Feature requests are always welcome. If you wish to contribute, please take a quick look at the [guidelines](./CONTRIBUTING.md)!

If there's anything you'd like to chat about, please feel free to send a message on my email  [Email](mailto:bouissihamza6@gmail.com)

## Contributors

This project follows the [all-contributors](https://github.com/kentcdodds/all-contributors) specification and is brought to you by these [awesome contributors](./CONTRIBUTORS.md).

## How to-use

Let's suppose you have an entity **Person ** :

```c#
public class Person
{
    private Person()
    {
        // This For EF Core
    }

    public int Id { get; set; }
    public string Name { get; set; } // this maximum length would be 25
    public int Age { get; set; }
    public string Email { get; set; }
    public string Username { get; set; } //this should be unique 
}
```

to  spice things up , let's add some constraint:

- Name maximum length: 25 
- Username should be unique

am fan of separating concerns ,so I'll create **PersonConfiguration** File to add those constraint:

```c#
public class PersonConfiguration: IEntityTypeConfiguration<Person>
{
   
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(p => p.Id); // this for primary key
        builder.HasIndex(p => p.Username).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(25);
    }
}
```

I think dotnet has make it simple enough to read the code 

to accept data from user we cannot use our entity class ,so  we need to create a DTO and a validator ...

#### Creating The DTO

PersonCreateIn(DTO):

```c#
public class PersonCreateIn
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
   
}
```

now our DTO is missing validations (Name maximum length, Username unique constraint )

#### Creating The Validator

adding validation would require installing **FluentValidation** 

ProductValidator:

```c#
public class PersonValidator : AbstractValidator<PersonCreateIn>
{
    public PersonValidator()
    {
        RuleFor(p => p.Name).Length(25);
        RuleFor(p => p.Username). // How we define unique constraint ?
    }
}
```

add unique constraint would require creating custom extension method for FluentValidation , I doubt you want that 

#### Welcome To My Procrastination World

 ##### Adding unique constraint

you don't have to build with all the mess of validating your property constraint or creating a custom method
we give you a  **IsUnique ** method:

```c#
public class PersonValidator : AbstractValidator<PersonCreateIn>
{
    public PersonValidator(ApplicationDbContext applicationDbContext)
    {
        var query = applicationDbContext.Set<Person>() as IQueryable; // <- add this also
        RuleFor(p => p.Name).Length(25);
        RuleFor(p => p.Username).IsUnique<PersonCreateIn, string, Person>(query,"Username"); // <- here is it 
    }
}
```

that was easy, is it ?!

but what if we add a foreign Key ?! how to validate it

##### Adding foreign-key constraint

before adding a foreign-key we need : 

- Create **Country** class 
- <u>Update</u> our "Product" class to point to "Country" 
- update PersonConfiguration to tell EF Core about our foreign-key

**Country class**

```c#
public class Country
{
    private Country()
    {
        // This For EF Core
    }
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Updated **Person class**

```c#
public class Person
{
    private Person()
    {
        // This For EF Core
    }

	... // truncated for readability 
    public Country Country { get; set; } // 
    public int CountryId { get; set; } // 
}
```

 Now let's update **PersonConfiguration**

```c#
public class PersonConfiguration: IEntityTypeConfiguration<Person>
{
   
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        ... // truncated for readability 
        builder.HasOne(p => p.Country); // this add foreign key
    }
}

```

**note**: add your migration and update database

Perfect .

now let's add our DTO property to verify foreign key

```c#
public class PersonCreateIn
{
    
	... // truncated for readability 
    public int CountryId { get; set; } // 
    
}
```

all good let's update our validation class 

```C#
public class ProductValidator : AbstractValidator<PersonCreateIn>
{
    public ProductValidator(ApplicationDbContext applicationDbContext)
    {
        ... // truncated for readability 
        var country_query = applicationDbContext.Set<Country>() as IQueryable;
        RuleFor(p => p.CountryId).IsForeignKey<PersonCreateIn, int, Country>(country_query);
    }
}
```

Simple is it ?! but if you're a big procrastinator like me you wouldn't take care if applying all these roles by yourself 

##### Auto Checking Class 

```c#
public class PersonCreateInValidator:EntityValidator<PersonCreateIn,Person,ApplicationDbContext> 
{
    public static ICollection<string> IgnoreList { get; set; } = new List<string>
    {

    };
    public static  IDictionary<string, string> FieldMappers = new Dictionary<string, string>()
    {
        
    };
    
    public PersonCreateInValidator(ApplicationDbContext context) : base(context, IgnoreList, FieldMappers)
    {
    }
}

```

EntityValidator : base class you should inherit from

so this will inspect your **PersonCreateIn** Dto  properties compare it with  **Person** Entity and add the needed constraints 

- Username unique constraint
- CountryId foreign Key constraint
- Name string max length

##### Customize your validation 

let's suppose you have DTO custom property name 

example best way to example

```c#
public class PersonCreateIn
{
    // public string Name { get; set; }
    public string FirstName { get; set; } // this should reference Name property
    public int Age { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }

    public int CountryId { get; set; }
    
}
```

if run your code your will stub into an Exception : <span style='color:red'>EntityColumnNotFoundException</span> this would tell **FirstName** is not defiend on **Person**

Solution:

```c#
public class PersonCreateInValidator:EntityValidator<PersonCreateIn,Person,ApplicationDbContext> 
{
    public static ICollection<string> IgnoreList { get; set; } = new List<string>
    {

    };
    public static  IDictionary<string, string> FieldMappers = new Dictionary<string, string>()
    {
        {"Name", "FirstName"}
    };
    
    public PersonCreateInValidator(ApplicationDbContext context) : base(context, IgnoreList, FieldMappers)
    {
    }
}

```

This will tell EntityValidator Person->Name mapped to PersonCreateIn->FirstName



## Backers [![Backers on Open Collective](https://opencollective.com/git-point/backers/badge.svg)](#backers)

Thank you to all our backers! 



## Sponsors [![Sponsors on Open Collective](https://opencollective.com/git-point/sponsors/badge.svg)](#sponsors)

Support this project by becoming a sponsor. Your logo will show up here with a link to your website. [[Become a sponsor](https://opencollective.com/git-point#sponsor)]



## Acknowledgments

