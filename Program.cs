using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace allocation
{
    

    class User
    {
        private static int COLOR_COUNT=20;
        int[] color = new int[User.COLOR_COUNT]; 
        double[] benifit = new double[User.COLOR_COUNT];
        int allocated;
        private int colorCount;
        private int bestColor;
        public int index;
        private static Random rnd = new Random();

        public User(int i)
        {
            colorCount=User.COLOR_COUNT;
            color = generateColorUsing();
            benifit = generateBenifit(color);
            allocated = -1;
            index = i;
        }

        public User(int[] c, double[] b,int i)
        {
            colorCount = User.COLOR_COUNT;
            color = c;
            benifit = b;
            allocated = -1;
            index = i;
        }

        private int[] generateColorUsing()
        {
            //Random rnd = new Random();
            int[] colorUse = new int[colorCount];
            for (int m = 0; m < colorCount; m++)
            {
                colorUse[m] = User.rnd.Next(2);
            }
            return colorUse;
        }

        private double[] generateBenifit(int[] color)
        {
            //Random rnd = new Random();
            double[] ben = new double[colorCount];
            for (int m = 0; m < colorCount; m++)
            {
                if (1 == color[m])
                  ben[m] = 5*User.rnd.NextDouble();
                else
                  ben[m] = 0;
            }
            return ben;
        }

        public void reset()
        {
            color = generateColorUsing();
            benifit = generateBenifit(color);
            allocated = -1;
        }

        public void reset(int[] c, double[] b)
        {
            color = c;
            benifit = b;
            allocated = -1;
        }

        public int[] getColor()
        {
            return color;
        }

        private void setBestColor(int m)
        {
            bestColor = m;
        }

        public int getBestResource()
        {
            return bestColor;
        }

        public double[] getBenifit()
        {
            return benifit;
        }

        public double maxBenifit()
        {
            double max = benifit[0];
            int index=0;
            for (int i = 1; i < colorCount; i++)
            {
                if (benifit[i]>max)
                {
                    max = benifit[i];
                    index = i;
                }
            }
            setBestColor(index);
            return max;
        }

        public void setAllocated(int a)
        {
            allocated = a;
        }
    }

    class Conflicts
    {
        Random rnd = new Random();
        public Conflicts(int count)
        {
            userCount = count;
            conflicts = generateConflicts();
        }


        private int[,] conflicts;
        private int userCount;
        private int[,] generateConflicts()
        {
            //Random rnd = new Random();
            int[,] c = new int[userCount,userCount];
            for (int i = 0; i < userCount; i++)
            {
                for (int j = 0; j < userCount; j++)
                {
                    if (i == j)
                    {
                        c[i, j] = -1;
                    }
                    else if (i > j)
                        c[i, j] = c[j, i];
                    else
                        c[i, j] = rnd.Next(2);
                }
            }
            return c;
        }

        public int[,] getConflicts()
        {
            return conflicts;
        }


    }

    class SpecAllocate
    {
        Random rnd = new Random();
        public SpecAllocate(int count)
        {
            userCount = count;
            users = generateUsers();
            conflicts = new Conflicts(userCount);
        }

        public SpecAllocate(int count, User[] u)
        {
            userCount = count;
            users = u;
            conflicts = new Conflicts(userCount);
        }

        public User[] users;
        public Conflicts conflicts;
        private int userCount;

        private User[] generateUsers()
        {
            User[] u = new User[userCount];
            for (int n = 0; n < userCount; n++)
            {
                u[n]=new User(n);
            }
            return u;
        }

        public void basicAllocate(User u,int m)
        {
            u.setAllocated(m);
        }

        public User getMaxBenUser(User[] users)
        {
            User max = users[0];
            for (int i = 1; i < userCount; i++)
            {
                if (users[i].maxBenifit()>max.maxBenifit())
                {
                    max = users[i];
                }
            }
            return max;
        }
        
        public List<User> unconfilictUsersFor(User u)
        {
            int j = 0;
            List<User> f=new List<User>();
            for (int i = 0; i < userCount; i++)
            {
                if (0==conflicts.getConflicts()[u.index,i])
                {
                    f.Add(users[i]);
                }
            }
            return f;
        }

        public List<User> findConflictUsersFor(User u)
        {
            List<User> c=new List<User>();
            for (int i = 0; i < userCount; i++)
            {
                if (1==conflicts.getConflicts()[u.index,i])
                {
                    c.Add(users[i]);
                }
            }
            return c;
        }
    }

    class Process
    {
        SpecAllocate sa;
        List<User> F;
        User[] allocatedUsers;
        public Process(int userCount)
        {
            sa = new SpecAllocate(userCount);
            allocatedUsers = sa.users;
        }

        List<List<User>> conflictList;

        public int run()
        {
            User currentUser;
            int resource = 0;
            currentUser = sa.getMaxBenUser(sa.users);
            resource = currentUser.getBestResource();
            sa.basicAllocate(currentUser, resource);
            F = sa.unconfilictUsersFor(currentUser);
                if (F.Count>0)
                {
                    foreach (var user in F)
                    {
                        subConfilcts(resource, user);  //将所有与最初分配的用户之间无干扰用户的干扰情况
                    }
                    mergeConflict();

                }
            
 
        }

        private void mergeConflict()
        {
            List<List<User>> toBeRemoved = new List<List<User>>();
            for (int i = 1; i < conflictList.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    int related = 0;
                    foreach (var u in conflictList[i])
                    {
                        if (conflictList[j].Contains(u))
                        {
                            related = 1;
                            break;
                        }
                    }
                    if (1 == related)
                    {
                        conflictList[j].AddRange(conflictList[i]);
                        toBeRemoved.Add(conflictList[i]);
                    }
                }
                foreach (var list in toBeRemoved)
                {
                    conflictList.Remove(list);
                }
            }
        }

        private void subConfilcts(int resource, User user)
        {
            List<User> conflictUsers = sa.findConflictUsersFor(user);
            List<User> cu = new List<User>();
            foreach (var u in F)
            {
                if (conflictUsers.Contains(u))
                {
                    cu.Add(u);
                }
            }
            if (0 != cu.Count)
            {
                cu.Add(user);
                conflictList.Add(cu);
            }
            else
                sa.basicAllocate(user, resource);
        }
 
    }

    //class DataSource //对外提供需要的矩阵B L
    //{
    //    public int[,] CreateFreeSpec()
    //    {
    //        Random rnd = new Random();
    //        int[,] freeSpec = new int[5, 20];

    //        for (int n = 0; n < 5; n++)
    //        {
    //            for (int m = 0; m < 20; m++)
    //            {
    //                int fsp = rnd.Next(2);
    //                freeSpec[n, m] = fsp;
    //            }
    //        }

    //        return freeSpec;
    //    }

    //    public double[,] CreateBenifit(int[,] freeSpectrum)
    //    {
    //        Random rnd=new Random();
    //        double[,] benifit = new double[5, 20];

    //        for (int n = 0; n < 5; n++)
    //        {
    //            for (int m = 0; m < 20; m++)
    //            {
    //                if (1 == freeSpectrum[n, m])
    //                {
    //                    double bef = 5 * rnd.NextDouble();
    //                    benifit[n, m] = bef;
    //                }
    //                else
    //                {
    //                    benifit[n, m] = 0;
    //                }
    //            }
    //        }
    //        return benifit;
    //    }

    //}

    //class SpecAllocation //分配算法
    //{
    //    DataSource dataSource = new DataSource();

    //    private int[,] freeSpec = new int[5, 20];
    //    public int[,] getFreeSpec()
    //    {
    //        return dataSource.CreateFreeSpec();
    //    }

    //    private double[,] benifit = new double[5, 20];
    //    public double[,] getBenifit()
    //    {
    //        return dataSource.CreateBenifit(freeSpec);
    //    }

    //    public int getMaxBenifit(int[,] benifit)
    //    {
    //        int max = 0;
    //        int[,] pos=new int[20,2];
    //        int a=0;
    //        int b = 0;
    //        for (int n = 0; n < 5; n++)
    //        {
    //            for (int m = 0; m < 20; m++)
    //            {
    //                if (max<benifit[n,m])
    //                {
    //                    max = benifit[n, m];
    //                    pos[a,b]=n;
    //                    b++;
    //                    pos[a, b] = m;
    //                    a++;
    //                }
    //            }
    //        }
            
    //    }
 
    //}



    class Program
    {
        static void Main(string[] args)
        {
            //Random rnd = new Random(); 
            //Random rnd1 = new Random();
            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine(rnd.NextDouble());
            //}
            
            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine(rnd.NextDouble());
            //}
            SpecAllocate sa = new SpecAllocate(5);
            User u ;
            int m = 0;
            u = sa.getMaxBenUser(sa.users);
            m=u.getBestResource();
            sa.basicAllocate(u,m);
            int[] f;
            f = sa.conflicts.unconfilictUsersFor(u);
            Console.ReadLine();
        }
    }
}
