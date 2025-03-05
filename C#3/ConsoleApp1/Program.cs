using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace praktos
{
    public class User
    {
        public string Username {get; set;}
        public string Password {get; set;}

    }  

    public class Taskpar
    {
        public string Title {get; set;}
        public string Description {get; set;}
        public string Priority {get; set;}
        public string Status {get; set;}
        public string Owner {get; set;}
    } 

    public class Reg
    {
        private string userfile = "user.txt";
        public async Task<bool> registr(string username, string password)
        {
            var users = await readuser();
            bool userexists = false;
            foreach (var user in users)
            {
                if (user.Username == username)
                {
                    userexists = true;
                    break;
                }
            }

            if (!userexists)
            {
                users.Add(new User { Username = username, Password = password });
                await writeusers(users);
                return true;
            }

            return false;
        }

        public async Task<User> login(string username, string password)
        {
            var users = await readuser();
            foreach (var user in users)
            {
                if (user.Username == username && user.Password == password)
                {
                    return user;
                }
            }
            return null;
        }

        public async Task<List<User>> readuser()
        {
            if (!File.Exists(userfile))
            {
                return new List<User>();
            }
            var lines = await File.ReadAllLinesAsync(userfile);
            var users = new List<User>();

            foreach (var line in lines)
            {
                var parts = line.Split('/');
                var user = new User
                {
                    Username = parts[0],
                    Password = parts[1]
                };
                users.Add(user);
            }
            return users;
        }

        private async Task writeusers(List<User> users)
        {
            var lines = new List<string>();

            foreach(var user in users)
            {
                var line = $"{user.Username}/{user.Password}";
                lines.Add(line);
            }
            
            await File.WriteAllLinesAsync(userfile, lines);
        }
    }
    
    public class Taskm
    {
        private string taskfile = "tasks.txt";

        public async Task<List<Taskpar>> gettask(string owner)
        {
            var tasks = await readtask();
            var userTasks = new List<Taskpar>();

            foreach (var task in tasks)
            {
                if (task.Owner == owner)
                {
                    userTasks.Add(task);
                }
            }

            return userTasks;
        }

        public async Task addtask(Taskpar task)
        {
            var tasks = await readtask();
            tasks.Add(task);
            await writetask(tasks);
        }

        public async Task updatetask(Taskpar task)
        {
            var tasks = await readtask();
            Taskpar existingTask = null;

            foreach (var t in tasks)
            {
                if (t.Title == task.Title && t.Owner == task.Owner)
                {
                    existingTask = t;
                    break;
                }
            }

            if (existingTask != null)
            {
                existingTask.Description = task.Description;
                existingTask.Priority = task.Priority;
                existingTask.Status = task.Status;
            }

            await writetask(tasks);
        }

        public async Task deletetask(string title, string owner)
        {
            var tasks = await readtask();

            for (int i = tasks.Count - 1; i >=0; i--)
            {
                if(tasks[i].Title == title && tasks[i].Owner == owner)
                {
                    tasks.RemoveAt(i);
                }
            }

            await writetask(tasks);
        }

        private async Task<List<Taskpar>> readtask()
        {
            if (!File.Exists(taskfile))
            {
                return new List<Taskpar>();
            }

            var lines = await File.ReadAllLinesAsync(taskfile);
            var tasks = new List<Taskpar>();

            foreach (var line in lines)
            {
                var parts = line.Split("/");
                var task = new Taskpar
                {
                    Title = parts[0],
                    Description = parts[1],
                    Priority = parts[2],
                    Status = parts[3],
                    Owner = parts[4]
                };
                tasks.Add(task);
            }
            return tasks;
        }

        private async Task writetask(List<Taskpar> tasks)
        {
            var lines = new List<string>();
            foreach (var task in tasks)
            {
                var line = $"{task.Title}/{task.Description}/{task.Priority}/{task.Status}/{task.Owner}";
                lines.Add(line);
            }
            await File.WriteAllLinesAsync(taskfile, lines);
        }
    }

    class Program
    {
        private static Reg reg = new Reg();
        private static Taskm taskm = new Taskm();
        private static User currentUser = null;
        
        static async Task Main(string[] args)
        {
            while (true)
            {
                if (currentUser == null)
                {
                    Console.WriteLine("1. Регестрация");
                    Console.WriteLine("2. Вход");
                    Console.WriteLine("3. Выход");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                    {
                        Console.Write("Введите имя пользователя: ");
                        var username = Console.ReadLine();
                        Console.Write("Введите пароль: ");
                        var password = Console.ReadLine();
                        if (await reg.registr(username, password))
                        {
                            currentUser = new User {Username = username, Password = password};
                            Console.Write("Вы вошли в приложение ");
                        }
                        else
                        {
                            Console.Write("Пользователь не найден");
                        }
                    } 
                    else if (choice == "2")
                    {
                        Console.Write("Введите имя пользователя: ");
                        var username = Console.ReadLine();
                        Console.Write("Введите пароль: ");
                        var password = Console.ReadLine();
                        currentUser = await reg.login(username, password);
                        if (currentUser != null)
                        {
                            Console.WriteLine("Вход выполнен.");
                        }
                        else
                        {
                            Console.WriteLine("Неверное имя пользователя или пароль.");
                        }
                    }
                    else if (choice == "3")
                    {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("1. Просмотреть задачи");
                    Console.WriteLine("2. Добавить задачу");
                    Console.WriteLine("3. Редактировать задачу");
                    Console.WriteLine("4. Удалить задачу");
                    Console.WriteLine("5. Выйти");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                    {
                        var tasks = await taskm.gettask(currentUser.Username);
                        foreach (var task in tasks)
                        {
                            Console.WriteLine($"Заголовок: {task.Title}, Описание: {task.Description}, Приоритет: {task.Priority}, Статус: {task.Status}");
                        }
                    }
                    else if (choice == "2")
                    {
                        Console.Write("Введите заголовок: ");
                        var title = Console.ReadLine();
                        Console.Write("Введите описание: ");
                        var description = Console.ReadLine();
                        Console.Write("Введите приоритет (низкий, средний, высокий): ");
                        var priority = Console.ReadLine();
                        Console.Write("Введите статус (недоступна, в процессе, завершена): ");
                        var status = Console.ReadLine();
                        await taskm.addtask(new Taskpar
                        { 
                            Title = title, 
                            Description = description, 
                            Priority = priority, 
                            Status = status, 
                            Owner = currentUser.Username 
                        });
                        Console.WriteLine("Задача добавлена.");
                    }
                    else if (choice == "3")
                    {
                        Console.Write("Введите заголовок задачи для редактирования: ");
                        var title = Console.ReadLine();
                        var tasks = await taskm.gettask(currentUser.Username);
                        var task = tasks.FirstOrDefault(t => t.Title == title);
                        if (task != null)
                        {
                            Console.Write("Введите новое описание: ");
                            task.Description = Console.ReadLine();
                            Console.Write("Введите новый приоритет (низкий, средний, высокий): ");
                            task.Priority = Console.ReadLine();
                            Console.Write("Введите новый статус (недоступна, в процессе, завершена): ");
                            task.Status = Console.ReadLine();
                            await taskm.updatetask(task);
                            Console.WriteLine("Задача обновлена.");
                        }
                        else
                        {
                            Console.WriteLine("Задача не найдена.");
                        }
                    }
                    else if (choice == "4")
                    {
                        Console.Write("Введите заголовок задачи для удаления: ");
                        var title = Console.ReadLine();
                        await taskm.deletetask(title, currentUser.Username);
                        Console.WriteLine("Задача удалена.");
                    }
                    else if (choice == "5")
                    {
                        currentUser = null;
                    }
                        
                }
            }
        }
    }
}
