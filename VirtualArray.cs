﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_methods_development_lab_1
{
    /// <summary>
    /// Виртуальный массив
    /// </summary>
    /// <typeparam name="T">Тип структуры элементов массива</typeparam>
    class VirtualArray<T> : IVirtualArray<T> where T : unmanaged
    {
        static readonly int _defaultBufferSize = 4;//Количество страниц в буфере
        static int _defaultSizePages = 512;//Размер страницы в байтах
        //Адресс файла
        static readonly string defaultFileName = "C:\\Users\\artur.ivanov.2021\\Documents\\ТД\\Технологии и методы программирования\\td1.bm";
        private FileInfo _swapFile; //Файл подкачки
        private page<T>[] _buffer; //Буфер страниц
        private long _size = 0; // Размер массива

        //Конструктор
        unsafe public VirtualArray(string fileName = null, long size = 10000)
        {
            _swapFile = new FileInfo(fileName != null || fileName == "" ? defaultFileName : fileName);
            //Если файл существует
            if (_swapFile.Exists)
            {
                BinaryReader br = new BinaryReader(_swapFile.OpenRead());
                //Проверяем сигнатуру
                if (br.ReadString() != "VM")
                    throw new FileLoadException("У файла нету требуемой сигнатуры");

                //Расчёт размера массива
                _size = br.BaseStream.Length - (System.Text.Encoding.Default.GetBytes("VM").Length+1);
                br.Close();
            }
            else
            {
                //Рассчитываем размер массива
                _size = (size + (_defaultSizePages - (size % _defaultSizePages)) % _defaultSizePages) * sizeof(T);

                //Проверяем место на диске
                DriveInfo driver = new DriveInfo(swapFile.FullName);
                if (driver.TotalFreeSpace < _size * sizeof(T))
                    throw new FileLoadException("Недостаточно места на диске");

                BinaryWriter bw = new BinaryWriter(_swapFile.Open(FileMode.CreateNew));

                //Создаём сигнатуру в начале файла
                bw.Write("VM");

                //Заполняем массива нулями
                for (long i = 0;
                    i < _size;
                    i++)
                    bw.Write(new byte[1] { 0b_0000_0000 }, 0, 1);
                bw.Close();
            }
            //Создаём буфер в оперативной памяти
            _buffer = new page<T>[_defaultBufferSize];
            for (int i = 0; i < _defaultBufferSize; i++)
                _buffer[i].fileIndex = -1;
        }
        public FileInfo swapFile { get => _swapFile; }
        public long size { get => _size; }
        /// <summary>
        ///Поиск страницы в буфере
        /// </summary>
        /// <param name="globalElementIndex">Индекс элемента в файле</param>
        /// <returns>Индекс страницы в буфере</returns>
        public unsafe int FindIndexPage(long globalElementIndex)
        {
            if (globalElementIndex < 0 || globalElementIndex >= _size)
                return -1;//Возвращает -1 если вышли за границы массива

            //Рассчитываем номер страницы в файле
            int filePageIndex = (int)(globalElementIndex / (_defaultSizePages / sizeof(T))), result = 0;

            //Проверяем, загружена ли нужная страница в буфер
            if (!_buffer.Any<page<T>>(p => p.fileIndex == filePageIndex))
            {
                //Если нет, то проверяем на наличие свободного места в буфере
                if (_buffer.Any<page<T>>(p => p.fileIndex == -1)) 
                {
                    while (_buffer[result].fileIndex != -1)
                        result++;
                    _buffer[result] = ReadPage(filePageIndex);
                }
                else
                {
                    //Если в буфере нету свободного места, то замещаем страницу к которой
                    //Давно не обращались
                    TimeSpan minTime = TimeSpan.MaxValue;
                    for(int i = 0; i<_defaultBufferSize; i++)
                        if (_buffer[i].timeOfModify < minTime)
                        {
                            minTime = _buffer[i].timeOfModify;
                            result = i;
                        }
                    //Если замещаемая страница была модифицированна, то
                    //Сохраняем её в файл перед замещением
                    if (_buffer[result].modify)
                        WritePage(ref _buffer[result]);
                    //Считываем нужную страницу
                    _buffer[result] = ReadPage(filePageIndex);
                }
            }
            else
            {
                //Выбираем нашу страницу в буфере
                while (_buffer[result].fileIndex != filePageIndex)
                    result++;
                //Обнавляем время обращения
                _buffer[result].timeOfModify = DateTime.Now.TimeOfDay;
            }

            return result;
        }
        public unsafe T this[int index]
        {
            get
            {
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException("Индекс вышел за пределы размера массива");
                
                int indexPage = FindIndexPage(index);//Получаем индекс страницы с нужным элементом
                int indexInBuffer = index % (_defaultSizePages/sizeof(T));

                int nullCount = 0;
                for (int i = 0; i < sizeof(T); i++)
                    if (_buffer[indexPage].bitMap[i + indexInBuffer * sizeof(T)] == 0b_0000_0000)
                        nullCount++;
                if(nullCount == sizeof(T))
                    throw new AccessViolationException("Попытка обращения к неинициализированному элементу");

                //Возвращаем требуемый элемент
                return _buffer[indexPage].elements[indexInBuffer];
            }
            set
            {
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException("Индекс вышел за пределы размера массива");

                //Получаем индекс страницы с нужным элементом
                int indexPage = FindIndexPage(index), elementIndex = index % (_defaultSizePages / sizeof(T));
                
                //Устанавливаем новое значение элемента
                _buffer[indexPage].elements[elementIndex] = value;
                
                //Обновляем байтовую (Она же битовая) карту
                byte[] byteValue = StructConverter.s_getBytes<T>(value);
                for (int i = 0; i < byteValue.Length; i++)
                    _buffer[indexPage].bitMap[i + elementIndex * byteValue.Length] = byteValue[i];

                //Устанавливаем флаг модификации
                _buffer[indexPage].modify = true;
            }
        }
        /// <summary>
        /// Записываение страницы в файл
        /// </summary>
        /// <param name="page">Страница</param>
        private unsafe void WritePage(ref page<T> page)
        {
            page.timeOfModify = DateTime.Now.TimeOfDay;
            page.modify = false;
            //Расчитываем положение страницы в файле
            int startWrite = page.fileIndex * _defaultSizePages + (System.Text.Encoding.Default.GetBytes("VA").Length+1);

            BinaryWriter bw = new BinaryWriter(_swapFile.Open(FileMode.Open));
            bw.BaseStream.Seek(startWrite, SeekOrigin.Begin);
            //Записываем
            bw.Write(page.bitMap, 0, page.bitMap.Length);

            bw.Close();
        }
        /// <summary>
        /// Чтение страницы из файла
        /// </summary>
        /// <param name="globalPageIndex"> Индекс страницы в файле </param>
        /// <returns>Страница из файла</returns>
        private unsafe page<T> ReadPage(int globalPageIndex)
        {
            page<T> page = default(page<T>);
            page.timeOfModify = DateTime.Now.TimeOfDay;
            page.fileIndex = globalPageIndex;

            //Расчитываем положение страницы в файле
            int sizeElements = sizeof(T), startRead = page.fileIndex * _defaultSizePages + (System.Text.Encoding.Default.GetBytes("VA").Length+1);

            BinaryReader br = new BinaryReader(_swapFile.Open(FileMode.Open));
            br.BaseStream.Seek(startRead, SeekOrigin.Begin);

            //Считываем байтовую карту из файла
            page.bitMap = br.ReadBytes(_defaultSizePages);
            page.elements = new T[_defaultSizePages / sizeElements];

            //Конвертируем байтовую
            //(Она же битовая карта, где 1 байт байтовой карты - 8 бит битовой карты)
            //карту в элементы массива
            for (int i = 0; i < page.elements.Length; i++)
            {
                byte[] tmpBytes = new byte[sizeElements];

                for (int j = 0; j < sizeElements; j++)
                    tmpBytes[j] = page.bitMap[i * sizeElements + j];

                page.elements[i] = StructConverter.s_createStruct<T>(tmpBytes);
            }

            br.Close();

            return page;
        }
        /// <summary>
        /// Сохранение модифицированных страниц в файл
        /// </summary>
        public void Save()
        {
            for (int i = 0; i < _defaultBufferSize; i++)
                if (_buffer[i].modify)
                    WritePage(ref _buffer[i]);
        }
        /// <summary>
        /// Деструктор на случай сборки мусора
        /// </summary>
        ~VirtualArray()
        {
            this.Save();
        }
    }
}
