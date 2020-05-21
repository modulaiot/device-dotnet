IModule {

}

IModuleLoader: IModule {
  IModule Load();
}

IModuleService: IModule {
  void Start();
  void Stop();
}

IModuleRunner: IModule {
  void Run();
}

IIntervalRunner: IModuleRunner {
  int Interval;
}

ISensor {
  string value;
}

ISwitch : ISensor {
  void On();
  void Off();
}
