using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NewDrink2.Models
{
    public class NewDrinkDB : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Identity> Identities { get; set; }
        public DbSet<UserCanDo> UserCanDoes { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<SweetType> SweetTypes { get; set; }
        public DbSet<IceType> IceTypes { get; set; }
        public DbSet<AddItemType> AddItemTypes { get; set; }
        public DbSet<SizeType> SizeTypes { get; set; }
        public DbSet<AddItemTypePrice> AddItemTypePrices { get; set; }
        public DbSet<SweetTable> SweetTables { get; set; }
        public DbSet<IceTable> IceTables { get; set; }
        public DbSet<SizeTable> SizeTables { get; set; }
        public DbSet<AddItemTable> AddItemTables { get; set; }
        public DbSet<MenuSweet> MenuSweets { get; set; }
        public DbSet<MenuSize> MenuSizes { get; set; }
        public DbSet<MenuIce> MenuIces { get; set; }
        public DbSet<MenuAddItem> MenuAddItems { get; set; }
        public DbSet<MenuDrink> MenuDrinks { get; set; }
        public DbSet<CreateBuyOrder_LeaderOrder> CreateBuyOrder_LeaderOrders { get; set; }
        public DbSet<CreateBuyOrder_Detail> CreateBuyOrder_Details { get; set; }
        public DbSet<CreateBuyOrder_MemberOrder> CreateBuyOrder_MemberOrders { get; set; }
        public DbSet<SendMessageView> SendMessageViews { get; set; }
        public DbSet<LeaderSendMessage> LeaderSendMessages { get; set; }
        public DbSet<IndexImage> IndexImages { get; set; }
        public DbSet<OrderEndingSend> OrderEndingSends { get; set; }
    }
}