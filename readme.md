# 单元测试实践总结-代码可测性
> 实际工程中，当单元测试的覆盖率达到一个量级（比如50%）时，想再往上提高，往往需要花费巨大的代价。为了实现代码的可测性，要求我们的代码能够满足:【待完善】  
 1)代码结构和依赖关系明确；  
 2)有效封装和隔离了**不可测点**；  
 3)面向接口编程，存在可用的依赖**注入点**。  

本文在实践的基础上，总结了单元测试的基本知识，举例说明了怎么样实现代码的可测性。主要用于个人工作记录目的，方便日后参考。

## 单元测试基本知识
* 单元测试应具有的特性：1.自动的、可重复的；2.容易实现；3.一旦写好，将来都可使用；4.任何人都可运行；5.单击一个按钮就可运行；6.可以快速地运行。   
* 一个单元测试通常包括以下三个主要部分，可以简记“3A”：1.Arrange: 配置对象，根据需要创建/Mock/设置对象；2.Act: 操控对象，执行要测试的逻辑单元；3.Assert: 通过断言Assert判断结果是否符合预期。   
* 所有测试之间应该没有任何依赖关系。
* 通常测试框架存在四种最基本的Hook方法：1）测试类/模块/程序集初始化时调用的方法；2）测试类/模块/程序集清理时调用的方法；3）每个测试前置状态设置方法；4）每个测试后置清理方法。
* 测试命名方式的参考准则：1）项目名：[被测项目].Tests；2）类名：[被测类]Tests；3）方法名：方法名_测试场景或条件_预期结果。  

## Stub与Mock
* Stub和Mock是单元测试中常用的两个基本概念。通常，Stub对象用于解除依赖，而Mock对象用于交互测试。   
* 桩对象（Stub） 是对系统中现有依赖项的一个替代品，可人为控制。通过使用桩对象，无需设计依赖项，即可对代码直接进行测试。  
* 模拟对象（Mock）是系统中的一个伪对象，用来决定一个单元测试是通过还是失败。它通过验证被测对象和伪对象之间是否进行预期的交互来判断。通常每个测试只有一个伪对象。 
* 模拟对象和桩对象的区别：1.桩对象不会使测试失败，模拟对象可以；2.使用桩对象时，断言是针对被测类执行的。借助桩对象，能够确保测试顺利运行。使用模拟对象时，被测类与模拟对象通信，模拟对象记录所有的消息。测试利用模拟对象来验证被测对象。   

## 为什么面向接口编程
* 从代码依赖角度，面向接口编程，可以实现控制翻转IOC，解除Client类对Service类的依赖，让他们共同依赖于他们之间接口，结合反射和IOC容器，可以做到完全解耦。    
* 在单元测试中，我们考虑面向接口编程的原因是，当前部分Mock框架只能做到对接口和类的虚方法进行Mock，主要是由于单元测试框架实现方式的限制。比如Moq和Rhnio.Mocks，实现Mock对象的方法都是利用了[Castle.Core](https://github.com/castleproject/Core)来创建动态代理对象，通过拦截器的概念来Mock操作和监听交互，为了实现代理对象对原始依赖对象的可替换性，代理对象必须继承自原始依赖对象，在这种模式下，只有虚方法和接口才可以被代理。因此，为了实现有效Mock，有必要面向接口编程。

## 实现代码可测性
> 实现代码的可测性是一个逐渐演进的过程，之所以不能一蹴而就，我认为根本原因在于在代码可测性与程序设计的基本原则并不完全吻合。本人目前已知的冲突点有：1.程序设计强调良好的封装，而可测要求可见，即向外暴露；2.程序设计中，解耦是为了可扩展性，接口的提取根据程序逻辑的扩展点（现有的扩展点，或未来可能的扩展点），而可测性要求全面的面向接口编程，要求完全封装不可测点，即使他们完全不存在扩展的可能性；3.安全性和维护性，代码的可测性，一定程度上，要求代码做出一些调整，比如条件编译、额外的注入点、visualable调整等等，而这些调整往往对功能没有作用，且有损安全性和可维护性。当然这些问题都可以归结于目前测试框架发展还不太完善。         
话说回来，为了实现代码的可测性，在实现基本逻辑的基础上，重点在于：**识别依赖**，**隔离依赖**，**设置依赖注入点**，对于不可测的操作，还需要进行封装。下面将针对文件操作和网络操作的例子进行具体介绍。     
[例子源码](https://github.com/xiong-ang/UT.Optimization.Example)    

#### 例子1-文件操作Mock
* 原始不可测代码：   
```C#
public bool InitializeConfigFile(string installedConfigFilePath, string userCofigFilePath)
{
    if (!File.Exists(installedConfigFilePath))
    {
        // Log Error
        return false;
    }

    if (!File.Exists(userCofigFilePath))
    {
        try
        {
            string userConfigFileDirectory = Path.GetDirectoryName(userCofigFilePath);
            if (string.IsNullOrWhiteSpace(userConfigFileDirectory)) return false;
            if (!Directory.Exists(userConfigFileDirectory))
                Directory.CreateDirectory(userConfigFileDirectory);

            File.Copy(installedConfigFilePath, userCofigFilePath);

            return true;
        }
        catch (Exception)
        {
            // Log Error
        }
    }

    return false;
}
```   
上面代码实现基本配置文件初始化逻辑：如果用户目录下配置文件不存在，将安装目录下的配置文件拷贝到用户目录。   
代码中有多处对静态类File和Directory的使用，没法直接测试。  
因此，需要对静态类封装与隔离。     

* 依赖的封装与隔离：    
```C#
public interface IFileSystemOperation
{
    bool FileExists(string path);
    void FileCopy(string sourceFileName, string destFileName);

    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
}
```      
这样，可以将被测方法InitializeConfigFile对静态类File和Directory的依赖，变成了对IFileSystemOperation的依赖。    
然后，在测试过程中，我们需要创建依赖的Stub对象，因此，还需要通过设置依赖注入点。   

* 设置注入点：   
```C#   
private readonly IFileSystemOperation _fileSystemOperator;

// Default
public OptimizedTarget()
{
    _fileSystemOperator = new FileSystemOperator();
}

// Used by unit test
public OptimizedTarget(IFileSystemOperation fileSystemOperator)
{
    this._fileSystemOperator = fileSystemOperator;
}
```
基于此，我们可以方便构造我们的测试代码。   
例如，如果installedConfigFilePath存在，userCofigFilePath不存在，此时需要拷贝installedConfigFilePath到userCofigFilePath，我们测试拷贝过程中，由于文件夹权限等问题，出现异常的Case。   

* 测试代码：   
```C#   
[TestMethod]
public void InitializeConfigFile_InstalledFileExists_UserFileNotExists_ThrowException_ReturnFalse()
{
    // Arrange
    string installedFilePath = "installedFilePath";
    string userFilePath = "userFilePath";
    IFileSystemOperation mockFileSystemOperator = MockRepository.GenerateMock<IFileSystemOperation>();
    mockFileSystemOperator.Stub(s => s.FileExists(installedFilePath)).Return(true);
    mockFileSystemOperator.Stub(s => s.FileExists(userFilePath)).Return(false);
    mockFileSystemOperator.Stub(s => s.FileCopy("", "")).IgnoreArguments().Throw(new Exception());
    OptimizedTarget target = new OptimizedTarget(mockFileSystemOperator);

    // Act
    bool throwExceptionResult = target.InitializeConfigFile(installedFilePath, userFilePath);

    // Assert
    mockFileSystemOperator.AssertWasCalled(s => s.FileCopy(installedFilePath, userFilePath));
    Assert.IsFalse(throwExceptionResult);
}
```
#### 例子2-网络操作Mock
* 原始不可测代码：   
```C#  
public bool SendRequest(HttpRequestMessage message, out string answer)
{
    // message setting
    // ...
    //

    answer = string.Empty;
    if (null == message) return false;

    try
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = client.SendAsync(message).Result;
            if (null == response) return false;

            answer = response.Content.ReadAsStringAsync().Result;
            return true;
        }
    }
    catch (Exception)
    {
        // Log Error
        return false;
    }
}
```   
该代码使用HttpClient发送网络请求，并同步返回结果。   
考虑到测试的可重复运行性以及测试机器网络环境，HttpClient的网络请求操作不可测，需要进一步封装与隔离。    

* 依赖的封装与隔离：   
```C#
public interface IHttpClientWrapper
{
    bool SendRequest(HttpRequestMessage message, out string answer);
}
```   
基于此，可以设置注入点，修改原始依赖。    

* 设置注入点：   
```C#
private readonly IFileSystemOperation _fileSystemOperator;

// Default
public OptimizedTarget()
{
    _httpClientWrapper = new HttpClientWrapper();
}

// Used by unit test
public OptimizedTarget(IHttpClientWrapper httpClientWrapper)
{
    this._httpClientWrapper = httpClientWrapper;
}

public bool SendRequest(HttpRequestMessage message, out string answer)
{
    // message setting
    // ...
    //

    answer = string.Empty;
    if (null == message) return false;

    try
    {
        return _httpClientWrapper.SendRequest(message, out answer);
    }
    catch (Exception)
    {
        // Log Error
        return false;
    }
}
```   

* 测试代码：   
下面举例测试HttpClient由于网络延时或异常，抛出异常的Case。
```C#
[TestMethod]
public void SendRequest_HttpClientThrowException_ReturnFalse()
{
    IHttpClientWrapper httpClientWrapper = MockRepository.GenerateStub<IHttpClientWrapper>();
    httpClientWrapper.Stub(s => s.SendRequest(null, out string _)).IgnoreArguments().Throw(new Exception());
    OptimizedTarget target = new OptimizedTarget(httpClientWrapper);

    bool throwExceptionResult = target.SendRequest(new HttpRequestMessage(), out string _);

    Assert.IsFalse(throwExceptionResult);
}
```
#### 静态类Mock  
* 静态类除了作为普通的Helper类，还有一个很大的好处是可以保持全局的状态，而且不受GC的影响。可是在单元测试的场景下，往往很难解除其依赖，需要对其进一步封装与隔离。  
* 对于静态类的Mock，其处理方法可以完全参考“例子1-文件操作Mock”对静态类File和Directory的处理。

## 有用链接
[上一篇UnitTest总结](https://github.com/xiong-ang/Library/blob/master/Src/UnitTest/.Net%E5%8D%95%E5%85%83%E6%B5%8B%E8%AF%95%E8%89%BA%E6%9C%AF.md)  
[Rhnio.Mocks基本介绍](https://wrightfully.com/using-rhino-mocks-quick-guide-to-generating-mocks-and-stubs/)    
[Rhnio.Mocks中GenerateStub与GenerateMock的区别](https://stackoverflow.com/questions/2536551/rhino-mocks-difference-between-generatestubt-generatemockt/2536570)  
[Mock文件IO静态类](https://stackoverflow.com/questions/6499871/mock-file-io-static-class-in-c-sharp)    
