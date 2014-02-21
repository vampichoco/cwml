# Welcome to cwml

cwml Or Cassandra/Custom Web Markup Language is a new way to write web sites, designed to be simple, portable and expandable for the requeriments of every indivual web site.

cwml is a compiler that converts a custom markup file into an HTML file. As the compiler es flexible, you can add elements and créate your own markup language. 

imagine for example that you want to display in web browser information about an user, you should write a lot of boring HTML code, But in cwml you can use your own markup, like this: 
`
<user nm="vampichoco" age="25" location="Somewhere" />
`
And then, the compiler will generate the right HTML provided by you. 

cwml also provides a set of basic output generation of HTML (wich is still being a uncomplete, but you can help us to add new HTML elements to the standard lib.

[Read the docs](https://github.com/vampichoco/cwml/wiki) 

CWML has an integrated query solution, so, for building basic dynamic sites that consume database, you don't have to use PHP, ASP.NET or any another language.
By default CWML supports MongoDB and you won't need to create or administrate a database, just be worried to insert and extract data with simple and powerful query language. 

this is an example of how a query on cwml looks like: 
`
<query collection="cassandra" presenceOf="@form/location">
	<condition>
		<equals name="location" value="@form/location" />
	</condition>
	<sort name="name" type="ascending" />
	<output inherits="div">
		<person>
			<Name>@name</Name>
			<Location>@location</Location>
		</person>
	</output>
</query>
`