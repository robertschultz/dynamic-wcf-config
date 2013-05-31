/// <summary>
/// Registers the service channels.
/// </summary>
public static void RegisterChannels()
{
    // look up system.serviceModel/client/endpoint in web.config
    Configuration config = WebConfigurationManager.OpenWebConfiguration("~/web.config");
    var section = config.GetSectionGroup("system.serviceModel") as ServiceModelSectionGroup;
    if (section != null)
    {
        if (section.Client != null)
        {
            if (section.Client.Endpoints != null)
            {
                for (var i = 0; i < section.Client.Endpoints.Count; i++)
                {
                    // register channel in DI with the contract type for each endpoint found
                    string assemblyQualifiedTypeName = TypeIdentifier.GetAssemblyQualifiedTypeName(section.Client.Endpoints[i].Contract);
                    Type contractType = Type.GetType(assemblyQualifiedTypeName, true);
                    if (contractType != null)
                    {
                        Type channelFactoryType = typeof(ChannelFactory<>).MakeGenericType(contractType);
                        var binding = new BasicHttpBinding();
                        var endpoint = new EndpointAddress(section.Client.Endpoints[i].Address);
                        var factory = Activator.CreateInstance(channelFactoryType, binding, endpoint);
                        MethodInfo method = channelFactoryType.GetMethod("CreateChannel", new Type[0]);

                        UnitySharedContainer.Container.RegisterType(
                         contractType,
                         new ContainerControlledLifetimeManager(),
                         new InjectionFactory(c => method.Invoke(factory, null)));
                    }
                }
            }
        }
    }
}