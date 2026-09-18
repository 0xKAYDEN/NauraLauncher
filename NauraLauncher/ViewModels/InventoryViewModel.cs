using System.Collections.ObjectModel;
using NauraLauncher.Core.Interfaces;
using NauraLauncher.Core.Services;
using NauraLauncher.Common;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;

namespace NauraLauncher.ViewModels;

public class InventoryViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    public InventoryViewModel() : this(new InventoryService()) { }

    public InventoryViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;

        Inventory = new ObservableCollection<InventoryItem>();
        FilteredInventory = new ObservableCollection<InventoryItem>();

        Categories = new ObservableCollection<string> { "All", "Weapon", "Armor", "Garment", "Gem", "Consumable", "Mount", "Other" };

        SelectCategoryCommand = new RelayCommand(p =>
        {
            if (p is string cat)
            {
                SelectedCategory = cat;
                ApplyFilter();
            }
        });

        SelectItemCommand = new RelayCommand(p =>
        {
            if (p is InventoryItem item)
                SelectedItem = item;
        });

        RefreshCommand = new RelayCommand(async () => await LoadInventoryAsync());
        SearchCommand = new RelayCommand(_ => ApplyFilter());
    }

    private int _userId;
    public int UserId
    {
        get => _userId;
        set
        {
            if (SetProperty(ref _userId, value))
            {
                _ = LoadInventoryAsync();
            }
        }
    }

    public ObservableCollection<InventoryItem> Inventory { get; }
    public ObservableCollection<InventoryItem> FilteredInventory { get; }

    public ObservableCollection<string> Categories { get; }

    private string _selectedCategory = "All";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    private InventoryItem? _selectedItem;
    public InventoryItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                ApplyFilter();
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string InventoryCount => $"{FilteredInventory.Count} ITEMS";

    public RelayCommand SelectCategoryCommand { get; }
    public RelayCommand SelectItemCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand SearchCommand { get; }

    public async Task LoadInventoryAsync()
    {
        if (UserId == 0) return;
        IsLoading = true;
        try
        {
            var items = await _inventoryService.GetUserInventoryAsync(UserId);
            Inventory.Clear();
            foreach (var item in items)
                Inventory.Add(item);

            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredInventory.Clear();
        var query = Inventory.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var lower = SearchQuery.ToLowerInvariant();
            query = query.Where(i => i.Name.ToLowerInvariant().Contains(lower) || i.Description.ToLowerInvariant().Contains(lower));
        }

        if (SelectedCategory != "All")
        {
            if (Enum.TryParse<ItemType>(SelectedCategory, out var type))
                query = query.Where(i => i.Type == type);
        }

        foreach (var item in query)
            FilteredInventory.Add(item);

        OnPropertyChanged(nameof(InventoryCount));
    }

    public List<InventoryItem> GetTradableItems()
    {
        return Inventory.Where(i => i.IsTradable && !i.IsBound && !i.IsLocked).ToList();
    }
}
