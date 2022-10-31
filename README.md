# InjeCtor
Another Dependency Injection framework.

Work in Progress.

This is a project for me to try something out and maybe learn new things. Unfortunately i cannot guarantee it to ever be finished.

# Usage

You can simply create a instance if `InjeCtor` and start setting the mappings on the `Mapper` instance.

`using IInjeCtor injeCtor = new InjeCtor.Core.InjeCtor();`<br/>
`// add mappings for interfaces to InjeCtor and set their creation instruction`<br/>
`injeCtor.Mapper.Add<IPlayer>().As<Player>(); // mapped to always create a new instance on creation`<br/>
`injeCtor.Mapper.Add<IUserInteraction>().AsSingleton<ConsoleUserInteraction>();`<br/>
`injeCtor.Mapper.Add<IRequestContext>().AsScopeSingleton<RequestContext>();`<br/>
`injeCtor.Mapper.Add<IShield>().As<RoundShield>();`<br/>
`injeCtor.Mapper.Add<IWeapon>().As<Sword>();`<br/>

To request a instance call `Get` on `InjeCtor`

`IUserInteraction userInteraction = injeCtor.Get<IUserInteraction>();`
<br/><br/><br/><br/><br/>

To get a new scope to get new instances which are set to `ScopeSingleton` use `CreateScope` from `InjeCtor`.

`using IScope scope = injeCtor.CreateScope();`

NOTE: `InjeCtor` and `Scope` are disposeable. When Dispose is called on them they also dispose all Singletons/ScopeSingletons which also implement `IDisposable`.

<br/><br/><br/><br/><br/>
It's also possible to use a `DynamicTypeMapper` to only register the interfaces which are going to be used and call a `Resolve` method to search for concrete implementations within AppDomain / AssemblyLoadContext.

NOTE: in this case the mapper needs to be instantiated first and passed to `InjeCtor`.

`DynamicTypeMapper mapper = new DynamicTypeMapper()`;<br/>
`mapper.Add<IPlayer>();`<br/>
`mapper.Add<IUserInteraction>().AsSingleton();`<br/>
`mapper.Add<IRequestContext>().AsScopeSingleton();`<br/>
`mapper.Add<IShield>();`<br/>
`mapper.Add<IWeapon>();`<br/>
`mapper.Resolve();`<br/>

Then create a `InjeCtor` instance and pass the mapper to it.

`using IInjeCtor injeCtor = new InjeCtor.Core.InjeCtor(mapper);`

<br/><br/><br/><br/><br/>
To Inject a Property to a concrete type mark the property with the `[Inject]` attribute.

`[Inject]`<br/>
`public IWeapon Weapon { get; set; }`

NOTE: In order to inject properties the Property needs to have a public setter at the moment and the type needs to be registered on the `ITypeInformationProvider`
interface which is passed to `InjeCtor`. In case of a default instantiation of `InjeCtor` there is no need for further actions since all added types with `injector.Mapper.Add(...)` will also be added to the `ITypeInformationProvider`. If you pass a mapping through the constructor it will also add the informations to the `ITypeInformationProvider` but ONLY if they are added AFTER instantiating `Injector`.
If you create a `ITypeMappingProvider`, add all mappings and then create `InjeCtor` with it then you need to add the types to `ITypeInformationProvider/ITypeInformationBuilder` yourself.

In case you provide a own implementation of `ITypeInformationProvider` also implement `ITypeInformationBuilder` to automatically add the types on a new mapping added.
