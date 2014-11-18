Queryish
========

Add Dynamic Query Powers to Entity Framework

Entity Framework rocks! But if you want to dynamically generate queries you are stuck with Expression Trees - yuck!
Queryish solves for this problem.  It is a simple and light weight solutions for creating dynamic queries in 
Entity Framework.


Queryish supports >,>=,<,<=, Not, Like, ==, and, or, order by, and paging.  Query plans are cached too.

Quick Start
---
Include Queryish as a reference.  Your project supplies the DbContext and connection string


Examples
---
Simple query to find elements
```c#

Filter filter = new Filter();            
filter.FilterCollection.Add(new FilterItem("Name", "June", "=="));
filter.FilterCollection.Add(new FilterItem("Name", "July", "=="));
filter.FilterCollection.Add(new FilterItem("Name", "Aug", "=="));

using (DatabaseContext context = new DatabaseContext(connectionString))
{
    IQueryable<TestCase> query = context.TestCases.AsQueryable();
    var queryMaker = new QueryFactory<TestCase>();
    var data = queryMaker.Queryable<TestCase>(query, filter, "Id");
    Assert.AreEqual(5, data.Count())
}
        
```
Set the filter Size and Page to page through the results.

```c#

Filter filter = new Filter();            
filter.Size = 5;
filter.Page = 1;

using (DatabaseContext context = new DatabaseContext(connectionString))
{
    IQueryable<TestCase> query = context.TestCases.AsQueryable();
    var queryMaker = new QueryFactory<TestCase>();
    var data = queryMaker.Queryable<TestCase>(query, filter, "Id");
    Assert.AreEqual(5, data.Count())
}
        
```


The MIT License (MIT)

Copyright (c) 2014 rocketcoder

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.


