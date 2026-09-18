using NauraLauncher.Application.Interfaces;
using NauraLauncher.Application.Services;
using NauraLauncher.Common;
using NauraLauncher.Domain.Entities;

namespace NauraLauncher.ViewModels;

public class AuthViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    public AuthViewModel() : this(new AuthService()) { }

    public AuthViewModel(IAuthService authService)
    {
        _authService = authService;

        LoginCommand = new RelayCommand(async () => await LoginAsync(), () => !IsBusy);
        RegisterCommand = new RelayCommand(async () => await RegisterAsync(), () => !IsBusy);
        SwitchToRegisterCommand = new RelayCommand(() => IsLoginMode = false);
        SwitchToLoginCommand = new RelayCommand(() => IsLoginMode = true);
        ClearErrorCommand = new RelayCommand(() => ErrorMessage = string.Empty);
    }

    // Mode
    private bool _isLoginMode = true;
    public bool IsLoginMode
    {
        get => _isLoginMode;
        set
        {
            if (SetProperty(ref _isLoginMode, value))
            {
                OnPropertyChanged(nameof(IsRegisterMode));
                ErrorMessage = string.Empty;
            }
        }
    }
    public bool IsRegisterMode => !IsLoginMode;

    // Login fields
    private string _loginUsername = string.Empty;
    public string LoginUsername
    {
        get => _loginUsername;
        set => SetProperty(ref _loginUsername, value);
    }

    private string _loginPassword = string.Empty;
    public string LoginPassword
    {
        get => _loginPassword;
        set => SetProperty(ref _loginPassword, value);
    }

    // Register fields
    private string _regUsername = string.Empty;
    public string RegUsername
    {
        get => _regUsername;
        set => SetProperty(ref _regUsername, value);
    }

    private string _regEmail = string.Empty;
    public string RegEmail
    {
        get => _regEmail;
        set => SetProperty(ref _regEmail, value);
    }

    private string _regDisplayName = string.Empty;
    public string RegDisplayName
    {
        get => _regDisplayName;
        set => SetProperty(ref _regDisplayName, value);
    }

    private string _regPassword = string.Empty;
    public string RegPassword
    {
        get => _regPassword;
        set => SetProperty(ref _regPassword, value);
    }

    private string _regConfirmPassword = string.Empty;
    public string RegConfirmPassword
    {
        get => _regConfirmPassword;
        set => SetProperty(ref _regConfirmPassword, value);
    }

    // State
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetProperty(ref _errorMessage, value);
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    private User? _authenticatedUser;
    public User? AuthenticatedUser
    {
        get => _authenticatedUser;
        set => SetProperty(ref _authenticatedUser, value);
    }

    public event EventHandler<User>? OnLoginSuccess;
    public event EventHandler<string>? OnError;

    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _authService.LoginAsync(new LoginRequest
            {
                Username = LoginUsername,
                Password = LoginPassword
            });

            if (result.Success && result.User != null)
            {
                AuthenticatedUser = result.User;
                OnLoginSuccess?.Invoke(this, result.User);
            }
            else
            {
                ErrorMessage = result.Message;
                OnError?.Invoke(this, result.Message);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            OnError?.Invoke(this, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegisterAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _authService.RegisterAsync(new RegisterRequest
            {
                Username = RegUsername,
                Email = RegEmail,
                DisplayName = RegDisplayName,
                Password = RegPassword,
                ConfirmPassword = RegConfirmPassword
            });

            if (result.Success && result.User != null)
            {
                AuthenticatedUser = result.User;
                OnLoginSuccess?.Invoke(this, result.User);
            }
            else
            {
                ErrorMessage = result.Message;
                OnError?.Invoke(this, result.Message);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            OnError?.Invoke(this, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Commands
    public RelayCommand LoginCommand { get; }
    public RelayCommand RegisterCommand { get; }
    public RelayCommand SwitchToRegisterCommand { get; }
    public RelayCommand SwitchToLoginCommand { get; }
    public RelayCommand ClearErrorCommand { get; }

    public void Reset()
    {
        LoginUsername = string.Empty;
        LoginPassword = string.Empty;
        RegUsername = string.Empty;
        RegEmail = string.Empty;
        RegDisplayName = string.Empty;
        RegPassword = string.Empty;
        RegConfirmPassword = string.Empty;
        ErrorMessage = string.Empty;
        IsLoginMode = true;
    }
}
